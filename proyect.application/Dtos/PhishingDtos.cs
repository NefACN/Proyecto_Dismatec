namespace proyect.application.Dtos
{
    public record PlantillaDto(
        int IdPlantilla,
        string Nombre,
        string Asunto,
        string NivelCriticidad
    );
    public record CrearPlantillaRequest(
        string Nombre,
        string AsuntoCorreo,
        string CuerpoHtml,
        int IdCriticidad,
        int IdCategoriaPregunta
    );

    public record CampaniaListItemDto(
        int IdCampania,
        string Titulo,
        string Plantilla,
        string Estado,
        DateTime? FechaInicio,
        DateTime? FechaFin,
        int TotalAreasSegmentadas
    );

    public record CampaniaDetalleDto(
        int IdCampania,
        string Titulo,
        string Plantilla,
        string Estado,
        string Creador,
        DateTime? FechaInicio,
        DateTime? FechaFin,
        List<string> AreasSegmentadas,
        int TotalEnviosGenerados,
        int TotalCorreosAbiertos,
        int TotalClicsVulnerados
    );

    public record CrearCampaniaRequest(
        string Titulo,
        int IdPlantilla,
        DateTime? FechaInicioProgramada,
        DateTime? FechaFinProgramada,
        List<int> AreasSeleccionadas
    );

}
