import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:meu_condominio/features/encomendas/data/models/encomenda_model.dart';
import 'package:meu_condominio/features/encomendas/data/repositories/encomenda_repository.dart';

// ──────────────────────────────────────────────────────────
// Events
// ──────────────────────────────────────────────────────────
sealed class EncomendaEvent extends Equatable {
  const EncomendaEvent();
  @override List<Object?> get props => [];
}

final class CarregarPendentesEvent extends EncomendaEvent {
  const CarregarPendentesEvent();
}

final class ProcessarOcrEvent extends EncomendaEvent {
  final String imagemPath;
  const ProcessarOcrEvent(this.imagemPath);
  @override List<Object?> get props => [imagemPath];
}

final class RegistrarEncomendaEvent extends EncomendaEvent {
  final String moradorId;
  final String tipoEncomenda;
  final String? textoOcrBruto;
  const RegistrarEncomendaEvent({
    required this.moradorId,
    required this.tipoEncomenda,
    this.textoOcrBruto,
  });
  @override List<Object?> get props => [moradorId, tipoEncomenda];
}

final class DarBaixaEvent extends EncomendaEvent {
  final String encomendaId;
  final String codigo;
  const DarBaixaEvent({required this.encomendaId, required this.codigo});
  @override List<Object?> get props => [encomendaId, codigo];
}

// ──────────────────────────────────────────────────────────
// States
// ──────────────────────────────────────────────────────────
sealed class EncomendaState extends Equatable {
  const EncomendaState();
  @override List<Object?> get props => [];
}

final class EncomendaInitial  extends EncomendaState { const EncomendaInitial(); }
final class EncomendaLoading  extends EncomendaState { const EncomendaLoading(); }

final class PendentesCarregadas extends EncomendaState {
  final List<EncomendaModel> pendentes;
  const PendentesCarregadas(this.pendentes);
  @override List<Object?> get props => [pendentes];
}

final class OcrProcessado extends EncomendaState {
  final OcrResultado resultado;
  const OcrProcessado(this.resultado);
  @override List<Object?> get props => [resultado];
}

final class EncomendaRegistrada extends EncomendaState {
  final String codigoRetirada;
  final bool   whatsAppEnviado;
  const EncomendaRegistrada({required this.codigoRetirada, required this.whatsAppEnviado});
  @override List<Object?> get props => [codigoRetirada];
}

final class BaixaConfirmada extends EncomendaState {
  final String encomendaId;
  const BaixaConfirmada(this.encomendaId);
  @override List<Object?> get props => [encomendaId];
}

final class EncomendaErro extends EncomendaState {
  final String mensagem;
  const EncomendaErro(this.mensagem);
  @override List<Object?> get props => [mensagem];
}

// ──────────────────────────────────────────────────────────
// BLoC
// ──────────────────────────────────────────────────────────
class EncomendaBloc extends Bloc<EncomendaEvent, EncomendaState> {
  final EncomendaRepository _repository;

  EncomendaBloc({required EncomendaRepository repository})
      : _repository = repository,
        super(const EncomendaInitial()) {
    on<CarregarPendentesEvent>(_onCarregarPendentes);
    on<ProcessarOcrEvent>(_onProcessarOcr);
    on<RegistrarEncomendaEvent>(_onRegistrar);
    on<DarBaixaEvent>(_onDarBaixa);
  }

  Future<void> _onCarregarPendentes(
      CarregarPendentesEvent event, Emitter<EncomendaState> emit) async {
    emit(const EncomendaLoading());
    try {
      final pendentes = await _repository.listarPendentes();
      emit(PendentesCarregadas(pendentes));
    } catch (e) {
      emit(EncomendaErro('Erro ao carregar pendentes: ${e.toString()}'));
    }
  }

  Future<void> _onProcessarOcr(
      ProcessarOcrEvent event, Emitter<EncomendaState> emit) async {
    emit(const EncomendaLoading());
    try {
      // Em produção: envie a imagem para o endpoint OCR do backend
      final resultado = await _repository.parseOcr(event.imagemPath);
      emit(OcrProcessado(resultado));
    } catch (e) {
      emit(EncomendaErro('Falha no OCR. Preencha manualmente.'));
    }
  }

  Future<void> _onRegistrar(
      RegistrarEncomendaEvent event, Emitter<EncomendaState> emit) async {
    emit(const EncomendaLoading());
    try {
      final resp = await _repository.registrar(
        moradorId:    event.moradorId,
        tipoEncomenda: event.tipoEncomenda,
        textoOcrBruto: event.textoOcrBruto,
      );
      emit(EncomendaRegistrada(
        codigoRetirada: resp.codigoRetirada,
        whatsAppEnviado: resp.whatsAppEnviado,
      ));
    } catch (e) {
      emit(EncomendaErro('Erro ao registrar encomenda: ${e.toString()}'));
    }
  }

  Future<void> _onDarBaixa(
      DarBaixaEvent event, Emitter<EncomendaState> emit) async {
    emit(const EncomendaLoading());
    try {
      await _repository.darBaixa(
        encomendaId:    event.encomendaId,
        codigoInformado: event.codigo,
      );
      emit(BaixaConfirmada(event.encomendaId));
    } catch (e) {
      emit(const EncomendaErro('Código inválido ou encomenda não encontrada.'));
    }
  }
}
