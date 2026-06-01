class UserSession {
  final String token;
  final String condominioId;
  final String role; // 'sindico' | 'porteiro'

  const UserSession({
    required this.token,
    required this.condominioId,
    required this.role,
  });

  bool get isSindico  => role == 'sindico';
  bool get isPorteiro => role == 'porteiro';

  factory UserSession.fromJson(Map<String, dynamic> json) => UserSession(
        token:        json['token']         as String,
        condominioId: json['condominioId']  as String,
        role:         json['role']          as String,
      );
}
