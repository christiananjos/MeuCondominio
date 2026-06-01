import 'package:flutter/material.dart';

/// Card exibido após o retorno do OCR, permitindo edição manual rápida
/// antes de o porteiro confirmar o registro.
class ConfirmacaoOcrCard extends StatelessWidget {
  final TextEditingController blocoCtrl;
  final TextEditingController aptoCtrl;
  final TextEditingController moradorIdCtrl;
  final bool ocrSucesso;

  const ConfirmacaoOcrCard({
    super.key,
    required this.blocoCtrl,
    required this.aptoCtrl,
    required this.moradorIdCtrl,
    required this.ocrSucesso,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      color: ocrSucesso
          ? const Color(0xFFF0FFF4)   // Verde suave
          : const Color(0xFFFFFAF0),  // Âmbar suave
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
        side: BorderSide(
          color: ocrSucesso ? const Color(0xFF38A169) : Colors.orange,
        ),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(children: [
              Icon(
                ocrSucesso ? Icons.auto_awesome : Icons.edit_note,
                color: ocrSucesso ? const Color(0xFF38A169) : Colors.orange,
                size: 20,
              ),
              const SizedBox(width: 6),
              Text(
                ocrSucesso
                    ? 'OCR detectou os dados abaixo'
                    : 'OCR parcial — confirme ou corrija',
                style: TextStyle(
                  fontWeight: FontWeight.w600,
                  color: ocrSucesso ? const Color(0xFF38A169) : Colors.orange,
                ),
              ),
            ]),
            const SizedBox(height: 14),

            // Bloco e Apartamento editáveis
            Row(children: [
              Expanded(
                child: TextFormField(
                  controller: blocoCtrl,
                  decoration: const InputDecoration(labelText: 'Bloco'),
                  textCapitalization: TextCapitalization.characters,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: TextFormField(
                  controller: aptoCtrl,
                  decoration: const InputDecoration(labelText: 'Apartamento'),
                  textCapitalization: TextCapitalization.characters,
                ),
              ),
            ]),
            const SizedBox(height: 12),

            // Campo para ID do morador (em produção: autocomplete por bloco/apto)
            TextFormField(
              controller: moradorIdCtrl,
              decoration: const InputDecoration(
                labelText:  'ID do Morador (busca por bloco/apto)',
                prefixIcon: Icon(Icons.person_search_outlined),
                helperText: 'Preenchido automaticamente após busca no servidor',
              ),
            ),
          ],
        ),
      ),
    );
  }
}
