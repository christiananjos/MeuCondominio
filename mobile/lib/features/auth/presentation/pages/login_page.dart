import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:meu_condominio/features/auth/presentation/bloc/auth_bloc.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _formKey     = GlobalKey<FormState>();
  final _emailCtrl   = TextEditingController();
  final _senhaCtrl   = TextEditingController();
  bool  _senhaOculta = true;

  @override
  void dispose() {
    _emailCtrl.dispose();
    _senhaCtrl.dispose();
    super.dispose();
  }

  void _submeter() {
    if (!_formKey.currentState!.validate()) return;
    context.read<AuthBloc>().add(
          AuthLoginEvent(
            email:    _emailCtrl.text.trim(),
            password: _senhaCtrl.text,
          ),
        );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: BlocConsumer<AuthBloc, AuthState>(
        listener: (context, state) {
          if (state is AuthErrorState) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Theme.of(context).colorScheme.error,
              ),
            );
          }
          // A navegação pós-login é gerida pelo AppRouter via BlocListener global
        },
        builder: (context, state) {
          final isLoading = state is AuthLoadingState;

          return SafeArea(
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      // Logo / Identidade
                      const Icon(Icons.apartment_rounded, size: 72, color: Color(0xFF1A56DB)),
                      const SizedBox(height: 12),
                      Text(
                        'MeuCondomínio',
                        textAlign: TextAlign.center,
                        style: Theme.of(context)
                            .textTheme
                            .headlineMedium
                            ?.copyWith(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Acesso à Portaria',
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.bodyLarge?.copyWith(color: Colors.grey),
                      ),
                      const SizedBox(height: 40),

                      // Campo E-mail
                      TextFormField(
                        controller:   _emailCtrl,
                        keyboardType: TextInputType.emailAddress,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                          labelText:  'E-mail',
                          prefixIcon: Icon(Icons.email_outlined),
                        ),
                        validator: (v) {
                          if (v == null || v.isEmpty) return 'Informe o e-mail.';
                          if (!v.contains('@'))       return 'E-mail inválido.';
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),

                      // Campo Senha
                      TextFormField(
                        controller:      _senhaCtrl,
                        obscureText:     _senhaOculta,
                        textInputAction: TextInputAction.done,
                        onFieldSubmitted: (_) => _submeter(),
                        decoration: InputDecoration(
                          labelText:  'Senha',
                          prefixIcon: const Icon(Icons.lock_outline),
                          suffixIcon: IconButton(
                            icon: Icon(
                              _senhaOculta ? Icons.visibility_off : Icons.visibility,
                            ),
                            onPressed: () => setState(() => _senhaOculta = !_senhaOculta),
                          ),
                        ),
                        validator: (v) {
                          if (v == null || v.length < 6) return 'Senha muito curta.';
                          return null;
                        },
                      ),
                      const SizedBox(height: 32),

                      // Botão Entrar (mín. 52dp conforme AppTheme)
                      ElevatedButton(
                        onPressed: isLoading ? null : _submeter,
                        child: isLoading
                            ? const SizedBox(
                                height: 22,
                                width:  22,
                                child:  CircularProgressIndicator(
                                  color:      Colors.white,
                                  strokeWidth: 2.5,
                                ),
                              )
                            : const Text('Entrar'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
