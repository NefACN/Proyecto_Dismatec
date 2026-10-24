namespace proyect.application.Dtos
{
    public record PreguntaDto(
        int IdPregunta,
        string TextoPregunta,
        string Explicacion,
        List<OpcionDto> Opciones
    );

    public record OpcionDto(
        int IdOpcion,
        string TextoOpcion
    );

    public record ValidarRespuestasRequest(
        List<int> IdsOpcionesSeleccionadas
    );

    public record CrearPreguntaRequest(
        string TextoPregunta,
        string ExplicacionEducativa,
        int IdCategoria,
        List<CrearOpcionRequest> Opciones
    );

    public record CrearOpcionRequest(
        string TextoOpcion,
        bool EsCorrecta
    );

    public record EditarPreguntasRequest(
        string TextoPregunta,
        string ExplicacionEducativa,
        int IdCategoria,
        List<EditarOpcionRequest> Opciones
    );

    public record EditarOpcionRequest(
        int? IdOpcion,
        string TextoOpcion,
        bool EsCorrecta
        );
}
