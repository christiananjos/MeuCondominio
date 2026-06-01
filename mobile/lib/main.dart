import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:meu_condominio/core/api/api_client.dart';
import 'package:meu_condominio/core/router/app_router.dart';
import 'package:meu_condominio/core/storage/secure_storage.dart';
import 'package:meu_condominio/core/theme/app_theme.dart';
import 'package:meu_condominio/features/auth/data/auth_repository.dart';
import 'package:meu_condominio/features/auth/presentation/bloc/auth_bloc.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const MeuCondominioApp());
}

class MeuCondominioApp extends StatelessWidget {
  const MeuCondominioApp({super.key});

  @override
  Widget build(BuildContext context) {
    // Injeção de dependências no topo da árvore de widgets
    final secureStorage = SecureStorage();
    final apiClient     = ApiClient(secureStorage: secureStorage);
    final authRepo      = AuthRepository(apiClient: apiClient, storage: secureStorage);

    return MultiRepositoryProvider(
      providers: [
        RepositoryProvider<AuthRepository>(create: (_) => authRepo),
        RepositoryProvider<ApiClient>(create: (_) => apiClient),
      ],
      child: BlocProvider(
        create: (ctx) => AuthBloc(
          authRepository: ctx.read<AuthRepository>(),
        )..add(const AuthCheckStatusEvent()),
        child: Builder(
          builder: (context) {
            final router = AppRouter(authBloc: context.read<AuthBloc>()).router;
            return MaterialApp.router(
              title: 'MeuCondomínio',
              theme: AppTheme.light,
              routerConfig: router,
              debugShowCheckedModeBanner: false,
            );
          },
        ),
      ),
    );
  }
}
