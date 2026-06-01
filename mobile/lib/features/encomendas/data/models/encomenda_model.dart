class EncomendaModel {
  final String  id;
  final String  moradorId;
  final String  nomeMorador;
  final String  bloco;
  final String  apartamento;
  final String  tipoEncomenda;
  final String  status;
  final DateTime dataRecebimento;
  final DateTime? dataRetirada;

  const EncomendaModel({
    required this.id,
    required this.moradorId,
    required this.nomeMorador,
    required this.bloco,
    required this.apartamento,
    required this.tipoEncomenda,
    required this.status,
    required this.dataRecebimento,
    this.dataRetirada,
  });

  factory EncomendaModel.fromJson(Map<String, dynamic> json) => EncomendaModel(
        id:              json['id']             as String,
        moradorId:       json['moradorId']      as String,
        nomeMorador:     json['nomeMorador']    as String,
        bloco:           json['bloco']          as String,
        apartamento:     json['apartamento']    as String,
        tipoEncomenda:   json['tipoEncomenda']  as String,
        status:          json['status']         as String,
        dataRecebimento: DateTime.parse(json['dataRecebimento'] as String),
        dataRetirada:    json['dataRetirada'] != null
            ? DateTime.parse(json['dataRetirada'] as String)
            : null,
      );

  /// Mascaramento LGPD para tela da portaria:
  /// "Chris Ferreira" -> "Chris F****"
  String get nomeMascarado {
    final partes = nomeMorador.trim().split(' ');
    if (partes.length == 1) return '${partes[0][0]}****';
    return '${partes[0]} ${partes[1][0]}****';
  }

  bool get isPendente   => status == 'Pendente'   || status == 'Notificado';
  bool get isRetirada   => status == 'Retirada';
  bool get isExtraviado => status == 'Extraviado';
}
