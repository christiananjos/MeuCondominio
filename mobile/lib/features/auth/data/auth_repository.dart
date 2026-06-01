import 'package:meu_condominio/core/api/api_client.dart';
import 'package:meu_condominio/core/storage/secure_storage.dart';
import 'package:meu_condominio/features/auth/domain/user_session.dart';

class AuthRepository {
  final ApiClient     _apiClient;
  final SecureStorage _storage;

  AuthRepository({required ApiClient apiClient, required SecureStorage storage})
      : _apiClient = apiClient,
        _storage   = storage;

  Future<UserSession> login(String email, String password) async {
    final response = await _apiClient.post<Map<String, dynamic>>(
      '/api/auth/login',
      data: {'email': email, 'password': password},
    );

    final data    = response.data!;
    final session = UserSession.fromJson(data);

    await _storage.salvarSessao(
      token:         session.token,
      condominioId:  session.condominioId,
      role:          session.role,
    );

    return session;
  }

  Future<UserSession?> recuperarSessao() async {
    final sessao = await _storage.lerSessao();
    if (sessao.token == null) return null;
    return UserSession(
      token:        sessao.token!,
      condominioId: sessao.condominioId ?? '',
      role:         sessao.role         ?? 'porteiro',
    );
  }

  Future<void> logout() => _storage.limpar();
}
