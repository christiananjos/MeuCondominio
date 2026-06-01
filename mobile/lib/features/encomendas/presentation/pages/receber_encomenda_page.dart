import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:image_picker/image_picker.dart';
import 'package:meu_condominio/features/encomendas/data/repositories/encomenda_repository.dart';
import 'package:meu_condominio/features/encomendas/presentation/bloc/encomenda_bloc.dart';
import 'package:meu_condominio/features/encomendas/presentation/widgets/confirmacao_ocr_card.dart';

/// Tela de Inclusão de Encomenda (Portaria)
/// Fluxo: Câmera → OCR → Confirmação → Salvar → WhatsApp disparado
class ReceberEncomendaPage extends StatefulWidget {
  const ReceberEncomendaPage({super.key});

  @override
  State<ReceberEncomendaPage> createState() => _ReceberEncomendaPageState();
}

class _ReceberEncomendaPageState extends State<ReceberEncomendaPage> {
  final _picker = ImagePicker();

  // Campos editáveis pelo porteiro após OCR
  final _blocoCtrl       = TextEditingController();
  final _aptoCtrl        = TextEditingController();
  final _moradorIdCtrl   = TextEditingController();
  String _tipoEncomenda  = 'Caixa';
  String? _textoOcrBruto;
  bool _ocrFeito         = false;

  @override
  void dispose() {
    _blocoCtrl.dispose();
    _aptoCtrl.dispose();
    _moradorIdCtrl.dispose();
    super.dispose();
  }

  // ──────────────────────────────────────────────────────────
  // Abre câmera e envia imagem para OCR no backend
  // ──────────────────────────────────────────────────────────
  Future<void> _abrirCamera() async {
    final imagem = await _picker.pickImage(
      source:     ImageSource.camera,
      imageQuality: 80,
      preferredCameraDevice: CameraDevice.rear,
    );
    if (imagem == null || !mounted) return;

    _textoOcrBruto = imagem.path;
    context.read<EncomendaBloc>().add(ProcessarOcrEvent(imagem.path));
  }

  void _submeter() {
    if (_moradorIdCtrl.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Selecione o morador antes de salvar.')),
      );
      return;
    }

    context.read<EncomendaBloc>().add(
          RegistrarEncomendaEvent(
            moradorId:    _moradorIdCtrl.text.trim(),
            tipoEncomenda: _tipoEncomenda,
            textoOcrBruto: _textoOcrBruto,
          ),
        );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Registrar Encomenda')),
      body: BlocConsumer<EncomendaBloc, EncomendaState>(
        listener: (ctx, state) {
          if (state is OcrProcessado && state.resultado.sucesso) {
            setState(() {
              _blocoCtrl.text = state.resultado.bloco ?? '';
              _aptoCtrl.text  = state.resultado.apartamento ?? '';
              _ocrFeito       = true;
            });
          }

          if (state is EncomendaRegistrada) {
            _mostrarSucessoDialog(
              context,
              codigoRetirada:  state.codigoRetirada,
              whatsEnviado:    state.whatsAppEnviado,
            );
          }

          if (state is EncomendaErro) {
            ScaffoldMessenger.of(ctx).showSnackBar(
              SnackBar(
                content: Text(state.mensagem),
                backgroundColor: Theme.of(ctx).colorScheme.error,
              ),
            );
          }
        },
        builder: (ctx, state) {
          final isLoading = state is EncomendaLoading;

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // ── BOTÃO PRINCIPAL: CÂMERA ─────────────────
                _BotaoCameraGrande(
                  onTap: isLoading ? null : _abrirCamera,
                ),
                const SizedBox(height: 24),

                // ── CARD DE CONFIRMAÇÃO OCR ──────────────────
                if (_ocrFeito || state is OcrProcessado)
                  ConfirmacaoOcrCard(
                    blocoCtrl:      _blocoCtrl,
                    aptoCtrl:       _aptoCtrl,
                    moradorIdCtrl:  _moradorIdCtrl,
                    ocrSucesso:     state is OcrProcessado
                        ? state.resultado.sucesso
                        : _ocrFeito,
                  ),

                if (!_ocrFeito)
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('Ou preencha manualmente:',
                              style: Theme.of(ctx).textTheme.titleSmall),
                          const SizedBox(height: 12),
                          Row(children: [
                            Expanded(
                              child: TextFormField(
                                controller: _blocoCtrl,
                                decoration: const InputDecoration(labelText: 'Bloco'),
                                textCapitalization: TextCapitalization.characters,
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: TextFormField(
                                controller: _aptoCtrl,
                                decoration: const InputDecoration(labelText: 'Apartamento'),
                                textCapitalization: TextCapitalization.characters,
                              ),
                            ),
                          ]),
                        ],
                      ),
                    ),
                  ),

                const SizedBox(height: 16),

                // ── TIPO DE ENCOMENDA ────────────────────────
                Card(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    child: DropdownButtonFormField<String>(
                      value: _tipoEncomenda,
                      decoration: const InputDecoration(
                        labelText: 'Tipo de Encomenda',
                        border: InputBorder.none,
                        filled: false,
                      ),
                      items: ['Caixa', 'Envelope', 'Outro']
                          .map((t) => DropdownMenuItem(value: t, child: Text(t)))
                          .toList(),
                      onChanged: (v) => setState(() => _tipoEncomenda = v ?? 'Caixa'),
                    ),
                  ),
                ),

                const SizedBox(height: 32),

                // ── BOTÃO SALVAR ─────────────────────────────
                ElevatedButton.icon(
                  onPressed: isLoading ? null : _submeter,
                  icon: isLoading
                      ? const SizedBox(
                          width: 20, height: 20,
                          child: CircularProgressIndicator(
                            color: Colors.white, strokeWidth: 2))
                      : const Icon(Icons.check_circle_outline),
                  label: Text(isLoading ? 'Registrando...' : 'Confirmar e Notificar Morador'),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  void _mostrarSucessoDialog(
    BuildContext context, {
    required String codigoRetirada,
    required bool whatsEnviado,
  }) {
    showDialog<void>(
      context: context,
      barrierDismissible: false,
      builder: (_) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Row(children: [
          Icon(Icons.check_circle, color: Color(0xFF38A169), size: 28),
          SizedBox(width: 8),
          Text('Encomenda Registrada!'),
        ]),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('Código de retirada:',
                style: Theme.of(context).textTheme.bodyMedium),
            const SizedBox(height: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
              decoration: BoxDecoration(
                color: const Color(0xFF1A56DB),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                codigoRetirada,
                style: const TextStyle(
                  fontSize:   32,
                  fontWeight: FontWeight.bold,
                  color:      Colors.white,
                  letterSpacing: 8,
                ),
              ),
            ),
            const SizedBox(height: 16),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  whatsEnviado ? Icons.check : Icons.warning_amber,
                  color: whatsEnviado ? const Color(0xFF38A169) : Colors.orange,
                  size: 18,
                ),
                const SizedBox(width: 6),
                Text(
                  whatsEnviado
                      ? 'WhatsApp enviado ao morador'
                      : 'WhatsApp não enviado (verifique config.)',
                  style: TextStyle(
                    color: whatsEnviado ? const Color(0xFF38A169) : Colors.orange,
                  ),
                ),
              ],
            ),
          ],
        ),
        actions: [
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              Navigator.of(context).pop(); // Volta para a Home da Portaria
            },
            child: const Text('Concluir'),
          ),
        ],
      ),
    );
  }
}

// ──────────────────────────────────────────────────────────
// Widget: Botão de câmera grande (zero atrito)
// ──────────────────────────────────────────────────────────
class _BotaoCameraGrande extends StatelessWidget {
  final VoidCallback? onTap;
  const _BotaoCameraGrande({this.onTap});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        height:      160,
        decoration: BoxDecoration(
          color:        const Color(0xFF1A56DB),
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: const Color(0xFF1A56DB).withOpacity(0.35),
              blurRadius: 16,
              offset: const Offset(0, 6),
            ),
          ],
        ),
        child: const Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.camera_alt_rounded, size: 56, color: Colors.white),
            SizedBox(height: 10),
            Text(
              'Fotografar Etiqueta',
              style: TextStyle(
                fontSize:   18,
                fontWeight: FontWeight.w600,
                color:      Colors.white,
              ),
            ),
            SizedBox(height: 4),
            Text(
              'OCR automático de Bloco e Apartamento',
              style: TextStyle(fontSize: 13, color: Colors.white70),
            ),
          ],
        ),
      ),
    );
  }
}
