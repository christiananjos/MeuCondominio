import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Abstração sobre FlutterSecureStorage para armazenamento seguro de tokens JWT.
class SecureStorage {
  static const String _tokenKey       = 'jwt_token';
  static const String _condominioKey  = 'condominio_id';
  static const String _roleKey        = 'user_role';

  final _storage = const FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
    iOptions: IOSOptions(accessibility: KeychainAccessibility.first_unlock),
  );

  Future<void> salvarToken(String token) =>
      _storage.write(key: _tokenKey, value: token);

  Future<String?> lerToken() => _storage.read(key: _tokenKey);

  Future<void> salvarSessao({
    required String token,
    required String condominioId,
    required String role,
  }) async {
    await Future.wait([
      _storage.write(key: _tokenKey,      value: token),
      _storage.write(key: _condominioKey, value: condominioId),
      _storage.write(key: _roleKey,       value: role),
    ]);
  }

  Future<({String? token, String? condominioId, String? role})> lerSessao() async {
    final results = await Future.wait([
      _storage.read(key: _tokenKey),
      _storage.read(key: _condominioKey),
      _storage.read(key: _roleKey),
    ]);
    return (token: results[0], condominioId: results[1], role: results[2]);
  }

  Future<void> limpar() => _storage.deleteAll();
}
