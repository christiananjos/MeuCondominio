import 'package:dio/dio.dart';
import 'package:meu_condominio/core/storage/secure_storage.dart';

/// Cliente HTTP centralizado. Injeta automaticamente o Bearer token
/// em todas as requisições autenticadas.
class ApiClient {
  static const String _baseUrl = 'https://api.meucondominio.app'; // Substitua pela URL real

  final SecureStorage _secureStorage;
  late final Dio _dio;

  ApiClient({required SecureStorage secureStorage}) : _secureStorage = secureStorage {
    _dio = Dio(BaseOptions(
      baseUrl:        _baseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 30),
      headers: {'Content-Type': 'application/json'},
    ));

    // Interceptor: injeta JWT automaticamente
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _secureStorage.lerToken();
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) {
          if (error.response?.statusCode == 401) {
            // Token expirado — NavKey para redirecionar ao Login
            // (implementar via GlobalKey<NavigatorState> ou stream de eventos)
          }
          handler.next(error);
        },
      ),
    );
  }

  Future<Response<T>> get<T>(String path, {Map<String, dynamic>? params}) =>
      _dio.get(path, queryParameters: params);

  Future<Response<T>> post<T>(String path, {Object? data}) =>
      _dio.post(path, data: data);

  Future<Response<T>> put<T>(String path, {Object? data}) =>
      _dio.put(path, data: data);
}
