import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:meu_condominio/features/auth/presentation/bloc/auth_bloc.dart';
import 'package:meu_condominio/features/auth/presentation/pages/login_page.dart';
import 'package:meu_condominio/features/encomendas/presentation/pages/pendentes_page.dart';
import 'package:meu_condominio/features/encomendas/presentation/pages/receber_encomenda_page.dart';

class AppRouter {
  final AuthBloc authBloc;

  AppRouter({required this.authBloc});

  late final GoRouter router = GoRouter(
    initialLocation: '/login',
    refreshListenable: GoRouterRefreshStream(authBloc.stream),
    redirect: (context, state) {
      final authState = authBloc.state;
      final isLoggingIn = state.matchedLocation == '/login';

      if (authState is AuthAuthenticated && isLoggingIn) {
        return authState.session.isSindico ? '/relatorios' : '/portaria';
      }
      if (authState is AuthUnauthenticated && !isLoggingIn) {
        return '/login';
      }
      return null;
    },
    routes: [
      GoRoute(path: '/login',     builder: (_, __) => const LoginPage()),
      GoRoute(path: '/portaria',  builder: (_, __) => const PortariaHomePage()),
      GoRoute(path: '/receber',   builder: (_, __) => const ReceberEncomendaPage()),
      GoRoute(path: '/pendentes', builder: (_, __) => const PendentesPage()),
      // Módulos futuros entram aqui sem tocar no núcleo:
      // GoRoute(path: '/avisos',  builder: (_, __) => const AvisosPage()),
      // GoRoute(path: '/reservas', builder: (_, __) => const ReservasPage()),
    ],
  );
}

/// Home da portaria: navegação por abas entre Receber e Pendentes
class PortariaHomePage extends StatefulWidget {
  const PortariaHomePage({super.key});
  @override State<PortariaHomePage> createState() => _PortariaHomePageState();
}

class _PortariaHomePageState extends State<PortariaHomePage> {
  int _tabIndex = 0;

  static const _tabs = [
    ReceberEncomendaPage(),
    PendentesPage(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _tabs[_tabIndex],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _tabIndex,
        onDestinationSelected: (i) => setState(() => _tabIndex = i),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.add_box_outlined),    label: 'Receber'),
          NavigationDestination(icon: Icon(Icons.inventory_2_outlined), label: 'Pendentes'),
        ],
      ),
    );
  }
}

/// Adaptador para usar stream do BLoC com GoRouter.refreshListenable
class GoRouterRefreshStream extends ChangeNotifier {
  GoRouterRefreshStream(Stream<dynamic> stream) {
    notifyListeners();
    _sub = stream.listen((_) => notifyListeners());
  }
  late final dynamic _sub;
  @override void dispose() { _sub.cancel(); super.dispose(); }
}
