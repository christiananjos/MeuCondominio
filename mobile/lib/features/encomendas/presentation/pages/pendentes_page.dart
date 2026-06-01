import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:meu_condominio/features/encomendas/data/models/encomenda_model.dart';
import 'package:meu_condominio/features/encomendas/presentation/bloc/encomenda_bloc.dart';

/// Tela: Lista de encomendas pendentes + Modal de "Dar Baixa"
class PendentesPage extends StatefulWidget {
  const PendentesPage({super.key});

  @override
  State<PendentesPage> createState() => _PendentesPageState();
}

class _PendentesPageState extends State<PendentesPage> {
  String _filtro = '';

  @override
  void initState() {
    super.initState();
    context.read<EncomendaBloc>().add(const CarregarPendentesEvent());
  }

  void _abrirModalBaixa(BuildContext context, EncomendaModel encomenda) {
    final codigoCtrl = TextEditingController();

    showModalBottomSheet<void>(
      context:       context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (sheetCtx) => BlocProvider.value(
        value: context.read<EncomendaBloc>(),
        child: Padding(
          padding: EdgeInsets.only(
            left: 24, right: 24, top: 24,
            bottom: MediaQuery.of(sheetCtx).viewInsets.bottom + 24,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text('Dar Baixa na Encomenda',
                  style: Theme.of(sheetCtx).textTheme.titleLarge),
              const SizedBox(height: 4),
              Text(
                'Bloco ${encomenda.bloco} / Ap ${encomenda.apartamento}  •  ${encomenda.nomeMascarado}',
                style: Theme.of(sheetCtx).textTheme.bodyMedium?.copyWith(color: Colors.grey),
              ),
              const SizedBox(height: 20),
              TextFormField(
                controller:     codigoCtrl,
                keyboardType:   TextInputType.number,
                maxLength:      4,
                textAlign:      TextAlign.center,
                style: const TextStyle(fontSize: 28, letterSpacing: 12, fontWeight: FontWeight.bold),
                decoration: const InputDecoration(
                  hintText:   '0000',
                  labelText:  'Código de 4 dígitos',
                  counterText: '',
                ),
              ),
              const SizedBox(height: 20),
              BlocConsumer<EncomendaBloc, EncomendaState>(
                listener: (ctx, state) {
                  if (state is BaixaConfirmada) {
                    Navigator.of(sheetCtx).pop();
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text('Encomenda retirada com sucesso!'),
                        backgroundColor: Color(0xFF38A169),
                      ),
                    );
                    context.read<EncomendaBloc>().add(const CarregarPendentesEvent());
                  }
                  if (state is EncomendaErro) {
                    ScaffoldMessenger.of(sheetCtx).showSnackBar(
                      SnackBar(
                        content:         Text(state.mensagem),
                        backgroundColor: Theme.of(ctx).colorScheme.error,
                      ),
                    );
                  }
                },
                builder: (ctx, state) {
                  final isLoading = state is EncomendaLoading;
                  return ElevatedButton(
                    onPressed: isLoading
                        ? null
                        : () {
                            if (codigoCtrl.text.length < 4) return;
                            ctx.read<EncomendaBloc>().add(DarBaixaEvent(
                              encomendaId: encomenda.id,
                              codigo:      codigoCtrl.text,
                            ));
                          },
                    child: isLoading
                        ? const CircularProgressIndicator(color: Colors.white)
                        : const Text('Confirmar Retirada'),
                  );
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Pendentes'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () =>
                context.read<EncomendaBloc>().add(const CarregarPendentesEvent()),
          ),
        ],
      ),
      body: Column(
        children: [
          // Barra de filtro
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 4),
            child: TextField(
              decoration: const InputDecoration(
                hintText:    'Filtrar por bloco, ap. ou nome...',
                prefixIcon:  Icon(Icons.search),
              ),
              onChanged: (v) => setState(() => _filtro = v.toLowerCase()),
            ),
          ),

          Expanded(
            child: BlocBuilder<EncomendaBloc, EncomendaState>(
              builder: (ctx, state) {
                if (state is EncomendaLoading) {
                  return const Center(child: CircularProgressIndicator());
                }

                if (state is EncomendaErro) {
                  return Center(child: Text(state.mensagem));
                }

                if (state is PendentesCarregadas) {
                  final lista = state.pendentes.where((e) {
                    if (_filtro.isEmpty) return true;
                    return e.bloco.toLowerCase().contains(_filtro)
                        || e.apartamento.toLowerCase().contains(_filtro)
                        || e.nomeMascarado.toLowerCase().contains(_filtro);
                  }).toList();

                  if (lista.isEmpty) {
                    return const Center(
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.inbox_outlined, size: 64, color: Colors.grey),
                          SizedBox(height: 12),
                          Text('Nenhuma encomenda pendente.', style: TextStyle(color: Colors.grey)),
                        ],
                      ),
                    );
                  }

                  return ListView.builder(
                    itemCount: lista.length,
                    itemBuilder: (_, i) => _EncomendaCard(
                      encomenda:  lista[i],
                      onDarBaixa: () => _abrirModalBaixa(context, lista[i]),
                    ),
                  );
                }

                return const SizedBox.shrink();
              },
            ),
          ),
        ],
      ),
    );
  }
}

class _EncomendaCard extends StatelessWidget {
  final EncomendaModel encomenda;
  final VoidCallback   onDarBaixa;

  const _EncomendaCard({required this.encomenda, required this.onDarBaixa});

  @override
  Widget build(BuildContext context) {
    final diasEspera = DateTime.now().difference(encomenda.dataRecebimento).inDays;

    return Card(
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: CircleAvatar(
          backgroundColor: diasEspera > 3
              ? const Color(0xFFF56565)
              : const Color(0xFF1A56DB),
          child: Text(
            encomenda.bloco,
            style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
          ),
        ),
        title: Text(
          'Ap ${encomenda.apartamento}  •  ${encomenda.nomeMascarado}',
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Text(
          '${encomenda.tipoEncomenda}  •  Aguardando há $diasEspera dia(s)',
          style: TextStyle(
            color: diasEspera > 3 ? const Color(0xFFF56565) : Colors.grey,
          ),
        ),
        trailing: ElevatedButton(
          onPressed: onDarBaixa,
          style: ElevatedButton.styleFrom(
            minimumSize: const Size(90, 40),
            padding: const EdgeInsets.symmetric(horizontal: 12),
          ),
          child: const Text('Dar Baixa'),
        ),
      ),
    );
  }
}
