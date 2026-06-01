import 'package:meu_condominio/core/api/api_client.dart';
import 'package:meu_condominio/features/encomendas/data/models/encomenda_model.dart';

class EncomendaRepository {
  final ApiClient _apiClient;

  EncomendaRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  Future<List<EncomendaModel>> listarPendentes() async {
    final response = await _apiClient.get<List<dynamic>>('/api/encomendas/pendentes');
    return (response.data as List)
        .map((e) => EncomendaModel.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<OcrResultado> parseOcr(String textoOcr) async {
    final response = await _apiClient.post<Map<String, dynamic>>(
      '/api/encomendas/ocr-parse',
      data: {'textoOcr': textoOcr},
    );
    return OcrResultado.fromJson(response.data!);
  }

  Future<EncomendaRegistradaResponse> registrar({
    required String moradorId,
    required String tipoEncomenda,
    String? textoOcrBruto,
  }) async {
    final response = await _apiClient.post<Map<String, dynamic>>(
      '/api/encomendas',
      data: {
        'moradorId':     moradorId,
        'tipoEncomenda': tipoEncomenda,
        'textoOcrBruto': textoOcrBruto,
      },
    );
    return EncomendaRegistradaResponse.fromJson(response.data!);
  }

  Future<void> darBaixa({
    required String encomendaId,
    required String codigoInformado,
  }) async {
    await _apiClient.post<void>(
      '/api/encomendas/$encomendaId/baixa',
      data: {'codigoInformado': codigoInformado},
    );
  }
}

class OcrResultado {
  final bool    sucesso;
  final String? bloco;
  final String? apartamento;
  final String? mensagem;

  const OcrResultado({
    required this.sucesso,
    this.bloco,
    this.apartamento,
    this.mensagem,
  });

  factory OcrResultado.fromJson(Map<String, dynamic> json) => OcrResultado(
        sucesso:      json['sucesso']      as bool,
        bloco:        json['bloco']        as String?,
        apartamento:  json['apartamento']  as String?,
        mensagem:     json['mensagem']     as String?,
      );
}

class EncomendaRegistradaResponse {
  final String encomendaId;
  final String codigoRetirada;
  final bool   whatsAppEnviado;

  const EncomendaRegistradaResponse({
    required this.encomendaId,
    required this.codigoRetirada,
    required this.whatsAppEnviado,
  });

  factory EncomendaRegistradaResponse.fromJson(Map<String, dynamic> json) =>
      EncomendaRegistradaResponse(
        encomendaId:    json['encomendaId']    as String,
        codigoRetirada: json['codigoRetirada'] as String,
        whatsAppEnviado: json['whatsAppEnviado'] as bool,
      );
}
