import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:meu_condominio/features/auth/data/auth_repository.dart';
import 'package:meu_condominio/features/auth/domain/user_session.dart';

// ──────────────────────────────────────────────────────────
// Events
// ──────────────────────────────────────────────────────────
sealed class AuthEvent extends Equatable {
  const AuthEvent();
  @override List<Object?> get props => [];
}

final class AuthCheckStatusEvent extends AuthEvent {
  const AuthCheckStatusEvent();
}

final class AuthLoginEvent extends AuthEvent {
  final String email;
  final String password;
  const AuthLoginEvent({required this.email, required this.password});
  @override List<Object?> get props => [email, password];
}

final class AuthLogoutEvent extends AuthEvent {
  const AuthLogoutEvent();
}

// ──────────────────────────────────────────────────────────
// States
// ──────────────────────────────────────────────────────────
sealed class AuthState extends Equatable {
  const AuthState();
  @override List<Object?> get props => [];
}

final class AuthInitialState     extends AuthState { const AuthInitialState(); }
final class AuthLoadingState     extends AuthState { const AuthLoadingState(); }
final class AuthUnauthenticated  extends AuthState { const AuthUnauthenticated(); }

final class AuthAuthenticated extends AuthState {
  final UserSession session;
  const AuthAuthenticated(this.session);
  @override List<Object?> get props => [session.token];
}

final class AuthErrorState extends AuthState {
  final String message;
  const AuthErrorState(this.message);
  @override List<Object?> get props => [message];
}

// ──────────────────────────────────────────────────────────
// BLoC
// ──────────────────────────────────────────────────────────
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final AuthRepository _authRepository;

  AuthBloc({required AuthRepository authRepository})
      : _authRepository = authRepository,
        super(const AuthInitialState()) {
    on<AuthCheckStatusEvent>(_onCheckStatus);
    on<AuthLoginEvent>(_onLogin);
    on<AuthLogoutEvent>(_onLogout);
  }

  Future<void> _onCheckStatus(AuthCheckStatusEvent event, Emitter<AuthState> emit) async {
    emit(const AuthLoadingState());
    final session = await _authRepository.recuperarSessao();
    if (session != null) {
      emit(AuthAuthenticated(session));
    } else {
      emit(const AuthUnauthenticated());
    }
  }

  Future<void> _onLogin(AuthLoginEvent event, Emitter<AuthState> emit) async {
    emit(const AuthLoadingState());
    try {
      final session = await _authRepository.login(event.email, event.password);
      emit(AuthAuthenticated(session));
    } catch (e) {
      emit(AuthErrorState('Credenciais inválidas. Verifique e tente novamente.'));
    }
  }

  Future<void> _onLogout(AuthLogoutEvent event, Emitter<AuthState> emit) async {
    await _authRepository.logout();
    emit(const AuthUnauthenticated());
  }
}
