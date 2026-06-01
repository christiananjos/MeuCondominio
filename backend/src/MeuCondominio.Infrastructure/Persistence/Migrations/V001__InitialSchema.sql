-- ============================================================
-- MEUCONDOMINIO - SCHEMA COMPLETO SUPABASE / POSTGRESQL
-- Versão: 1.0.0 | Multi-Tenant SaaS
-- ============================================================

-- Extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- TABELA: condominios  (Os Tenants / Clientes SaaS)
-- ============================================================
CREATE TABLE condominios (
    id              UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    nome_comercial  VARCHAR(200) NOT NULL,
    cnpj            VARCHAR(18),
    ativo           BOOLEAN     NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE condominios IS 'Tabela raiz de tenants. Cada condomínio é um cliente SaaS independente.';

-- ============================================================
-- TABELA: whatsapp_config  (Credenciais Meta por Tenant)
-- ============================================================
CREATE TABLE whatsapp_config (
    id                UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominio_id     UUID        NOT NULL REFERENCES condominios(id) ON DELETE CASCADE,
    meta_token        TEXT        NOT NULL,           -- Token Bearer da API Cloud da Meta
    phone_number_id   VARCHAR(100) NOT NULL,          -- ID do número de disparo na Meta
    template_name     VARCHAR(100) NOT NULL DEFAULT 'encomenda_recebida',
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_whatsapp_config_condominio UNIQUE (condominio_id)
);

COMMENT ON COLUMN whatsapp_config.meta_token IS 'Token de acesso permanente gerado no Meta Business Manager.';

-- ============================================================
-- TABELA: perfis_usuarios  (Extensão do Supabase auth.users)
-- ============================================================
CREATE TABLE perfis_usuarios (
    id              UUID        PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
    condominio_id   UUID        NOT NULL REFERENCES condominios(id) ON DELETE CASCADE,
    nome            VARCHAR(150) NOT NULL,
    role            VARCHAR(20) NOT NULL CHECK (role IN ('sindico', 'porteiro')),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

COMMENT ON TABLE perfis_usuarios IS 'Estende auth.users com dados de negócio: role e vínculo ao tenant.';

-- ============================================================
-- TABELA: moradores
-- ============================================================
CREATE TABLE moradores (
    id                UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominio_id     UUID        NOT NULL REFERENCES condominios(id) ON DELETE CASCADE,
    bloco             VARCHAR(10) NOT NULL,
    apartamento       VARCHAR(10) NOT NULL,
    nome_responsavel  VARCHAR(150) NOT NULL,
    whatsapp          VARCHAR(20) NOT NULL,  -- Formato internacional: 5511999991234
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_morador_unidade UNIQUE (condominio_id, bloco, apartamento)
);

COMMENT ON COLUMN moradores.whatsapp IS 'Número no formato internacional E.164 sem o "+". Ex: 5511999991234';

-- ============================================================
-- TABELA: encomendas
-- ============================================================
CREATE TABLE encomendas (
    id                UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    condominio_id     UUID        NOT NULL REFERENCES condominios(id) ON DELETE CASCADE,
    morador_id        UUID        NOT NULL REFERENCES moradores(id),
    tipo_encomenda    VARCHAR(20) NOT NULL CHECK (tipo_encomenda IN ('Caixa', 'Envelope', 'Outro')),
    status            VARCHAR(20) NOT NULL DEFAULT 'Pendente'
                        CHECK (status IN ('Pendente', 'Notificado', 'Retirada', 'Extraviado')),
    codigo_retirada   VARCHAR(4)  NOT NULL,
    data_recebimento  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    data_retirada     TIMESTAMPTZ
);

-- ============================================================
-- ÍNDICES DE PERFORMANCE
-- ============================================================
CREATE INDEX idx_encomendas_condominio_status
    ON encomendas(condominio_id, status);

CREATE INDEX idx_encomendas_morador
    ON encomendas(morador_id);

CREATE INDEX idx_encomendas_data_recebimento
    ON encomendas(condominio_id, data_recebimento DESC);

CREATE INDEX idx_moradores_condominio
    ON moradores(condominio_id);

CREATE INDEX idx_moradores_busca_unidade
    ON moradores(condominio_id, bloco, apartamento);

CREATE INDEX idx_perfis_condominio
    ON perfis_usuarios(condominio_id);

-- ============================================================
-- ROW LEVEL SECURITY (RLS) - Isolamento Multi-Tenant
-- ============================================================

ALTER TABLE condominios      ENABLE ROW LEVEL SECURITY;
ALTER TABLE perfis_usuarios  ENABLE ROW LEVEL SECURITY;
ALTER TABLE moradores        ENABLE ROW LEVEL SECURITY;
ALTER TABLE encomendas       ENABLE ROW LEVEL SECURITY;
ALTER TABLE whatsapp_config  ENABLE ROW LEVEL SECURITY;

-- ──────────────────────────────────────────────────────────
-- Funções auxiliares de contexto de segurança
-- (SECURITY DEFINER garante execução com permissões elevadas)
-- ──────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION get_condominio_id()
RETURNS UUID
LANGUAGE sql STABLE SECURITY DEFINER
AS $$
    SELECT condominio_id
    FROM   perfis_usuarios
    WHERE  id = auth.uid()
    LIMIT  1;
$$;

CREATE OR REPLACE FUNCTION get_user_role()
RETURNS VARCHAR
LANGUAGE sql STABLE SECURITY DEFINER
AS $$
    SELECT role
    FROM   perfis_usuarios
    WHERE  id = auth.uid()
    LIMIT  1;
$$;

-- ──────────────────────────────────────────────────────────
-- POLÍTICAS: perfis_usuarios
-- ──────────────────────────────────────────────────────────
CREATE POLICY "perfis_select_proprio"
    ON perfis_usuarios FOR SELECT
    USING (id = auth.uid());

CREATE POLICY "perfis_update_proprio"
    ON perfis_usuarios FOR UPDATE
    USING (id = auth.uid());

-- ──────────────────────────────────────────────────────────
-- POLÍTICAS: moradores
-- ──────────────────────────────────────────────────────────
CREATE POLICY "moradores_select_mesmo_condominio"
    ON moradores FOR SELECT
    USING (condominio_id = get_condominio_id());

CREATE POLICY "moradores_insert_sindico"
    ON moradores FOR INSERT
    WITH CHECK (
        condominio_id = get_condominio_id()
        AND get_user_role() = 'sindico'
    );

CREATE POLICY "moradores_update_sindico"
    ON moradores FOR UPDATE
    USING (
        condominio_id = get_condominio_id()
        AND get_user_role() = 'sindico'
    );

CREATE POLICY "moradores_delete_sindico"
    ON moradores FOR DELETE
    USING (
        condominio_id = get_condominio_id()
        AND get_user_role() = 'sindico'
    );

-- ──────────────────────────────────────────────────────────
-- POLÍTICAS: encomendas
-- ──────────────────────────────────────────────────────────
CREATE POLICY "encomendas_select_mesmo_condominio"
    ON encomendas FOR SELECT
    USING (condominio_id = get_condominio_id());

CREATE POLICY "encomendas_insert_operadores"
    ON encomendas FOR INSERT
    WITH CHECK (
        condominio_id = get_condominio_id()
        AND get_user_role() IN ('porteiro', 'sindico')
    );

CREATE POLICY "encomendas_update_operadores"
    ON encomendas FOR UPDATE
    USING (
        condominio_id = get_condominio_id()
        AND get_user_role() IN ('porteiro', 'sindico')
    );

-- ──────────────────────────────────────────────────────────
-- POLÍTICAS: whatsapp_config (apenas síndico)
-- ──────────────────────────────────────────────────────────
CREATE POLICY "whatsapp_config_sindico"
    ON whatsapp_config FOR ALL
    USING (
        condominio_id = get_condominio_id()
        AND get_user_role() = 'sindico'
    )
    WITH CHECK (
        condominio_id = get_condominio_id()
        AND get_user_role() = 'sindico'
    );

-- ──────────────────────────────────────────────────────────
-- TRIGGER: Auto-criar perfil ao registrar usuário no Auth
-- Os metadados condominio_id, nome e role devem ser passados
-- no campo `options.data` do supabase.auth.signUp()
-- ──────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION handle_new_user()
RETURNS TRIGGER
LANGUAGE plpgsql SECURITY DEFINER
SET search_path = public
AS $$
BEGIN
    INSERT INTO perfis_usuarios (id, condominio_id, nome, role)
    VALUES (
        NEW.id,
        (NEW.raw_user_meta_data->>'condominio_id')::UUID,
        COALESCE(NEW.raw_user_meta_data->>'nome', NEW.email),
        COALESCE(NEW.raw_user_meta_data->>'role', 'porteiro')
    );
    RETURN NEW;
END;
$$;

CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION handle_new_user();

-- ──────────────────────────────────────────────────────────
-- TRIGGER: Atualizar updated_at automaticamente
-- ──────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_whatsapp_config_updated_at
    BEFORE UPDATE ON whatsapp_config
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
