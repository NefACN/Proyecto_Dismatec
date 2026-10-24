import { icon } from '../../components/icons.js';

// Presentation only: the analysis API has not been implemented yet.
const root = document.getElementById('http-content');
const page = document.body.dataset.page;
const card = (title, text) => `<section class="panel"><h2>${title}</h2><div>${text}</div></section>`;
const table = headings => `<div class="table-scroll"><table class="event-table"><thead><tr>${headings.map(text => `<th>${text}</th>`).join('')}</tr></thead><tbody><tr><td colspan="${headings.length}" class="table-state">Sin datos disponibles. La integración del análisis está pendiente.</td></tr></tbody></table></div>`;
const metric = title => `<article class="metric"><div class="metric-top">${title}</div><strong aria-label="Sin datos">—</strong><small>Pendiente de integración</small></article>`;
const pages = {
    'http-analisis': ['Panel de análisis HTTP', 'Resumen del comportamiento del servidor.',
        `<div class="metric-grid">${['Registros procesados', 'Ventanas atípicas', 'Última ejecución'].map(metric).join('')}</div>` +
        `<div class="http-grid">${card('Evolución de eventos atípicos', 'El gráfico estará disponible cuando existan ejecuciones.')}${card('Distribución por clúster', 'Sin agrupaciones disponibles.')}</div>` +
        card('Eventos recientes', table(['Ventana', 'Ruta', 'Método', 'Respuesta', 'Revisión'])) +
        `<div class="quick-grid"><a href="http-patrones.html">${icon('layers')}Patrones de comportamiento</a><a href="http-eventos.html">${icon('activity')}Eventos atípicos</a><a href="http-ejecuciones.html">${icon('refresh')}Ejecuciones</a></div>`],
    'http-fuentes': ['Fuentes y registros', 'Recepción de registros nuevos y archivos históricos.',
        `<div class="http-grid">${card('Datos nuevos de NGINX', 'Conexión pendiente. Último registro recibido: no disponible.')}${card('Archivos históricos', 'La importación estará disponible al integrar el servicio de recepción.')}</div>` +
        `<div class="metric-grid">${['Registros recibidos', 'Registros válidos', 'Registros rechazados'].map(metric).join('')}</div>` +
        card('Periodo disponible', 'No hay registros recibidos.') + card('Errores de procesamiento', table(['Fecha', 'Fuente', 'Descripción']))],
    'http-ejecuciones': ['Ejecuciones del análisis', 'Consulta de periodos y resultados del análisis por lotes.',
        `<button class="btn btn-primary" disabled>Iniciar análisis por lotes</button>` + card('Historial de ejecuciones', table(['Identificador', 'Periodo', 'Fecha', 'Estado', 'Registros', 'Ruido', 'Versión', 'Parámetros']))],
    'http-patrones': ['Patrones de comportamiento', 'Agrupaciones de ventanas temporales y características predominantes.',
        card('Clústeres de la ejecución', table(['Clúster', 'Ventanas', 'Características', 'Eventos'])) +
        `<div class="http-grid">${card('Distribución de variables', 'Selecciona una ejecución cuando existan resultados.')}${card('Registros clasificados como ruido', 'El ruido se mostrará separado de los grupos.')}</div>`],
    'http-eventos': ['Eventos atípicos', 'Revisión de ventanas clasificadas como ruido.',
        `<div class="http-explainer">${icon('alert')}<p>Un evento atípico requiere revisión. Su clasificación como ruido no confirma un incidente.</p></div>` +
        `<fieldset disabled><legend>Filtros de eventos</legend><div class="http-context"><label>Desde<input type="date"></label><label>Hasta<input type="date"></label><label>Clúster<select><option>Sin clústeres disponibles</option></select></label><label>Ruta<input type="search" placeholder="Buscar ruta"></label><button class="btn">Filtrar</button></div></fieldset>` +
        card('Eventos para revisar', table(['Fecha', 'Ruta', 'Método HTTP', 'Respuesta', 'Análisis', 'Revisión', 'Observaciones']))],
    'http-comparacion': ['Comparación de patrones', 'Cambios entre ejecuciones y posibles agrupaciones nuevas.',
        `<fieldset disabled><legend>Ejecuciones a comparar</legend><div class="http-context"><label>Primera ejecución<select><option>Sin ejecuciones disponibles</option></select></label><label>Segunda ejecución<select><option>Sin ejecuciones disponibles</option></select></label></div></fieldset>` +
        card('Comparabilidad de periodos', 'Se verificará la duración de las ventanas, las variables, el escalado y las versiones del modelo.') +
        card('Cambios en agrupaciones y proporción de ruido', table(['Patrón', 'Primer periodo', 'Segundo periodo', 'Cambio']))],
    'http-modelo': ['Configuración del modelo', 'Parámetros y publicación controlada de versiones de HDBSCAN.',
        card('Modelo activo', 'Sin conexión con el servicio de modelos. No hay una versión activa disponible para consultar.') +
        `<fieldset disabled><legend>Parámetros de HDBSCAN</legend><div class="http-context"><label>min_cluster_size<input type="number" min="2"></label><label>min_samples<input type="number" min="1"></label><label>Método de escalado<select><option>Pendiente de configuración</option></select></label></div><p>Las variables y el conjunto de referencia se definirán al integrar el modelo.</p><button class="btn">Guardar configuración</button> <button class="btn">Evaluar</button> <button class="btn">Validar</button> <button class="btn">Publicar versión</button></fieldset>` +
        card('Historial de versiones', table(['Versión', 'Fecha', 'Parámetros', 'Evaluación', 'Estado'])) +
        card('Cambio de modelo activo', 'La publicación requerirá validación y confirmación explícita cuando el servicio esté disponible.')],
};
const [title, subtitle, content] = pages[page];
root.innerHTML = `<div class="http-integration-notice" role="status">${icon('layers')}<div><strong>Integración pendiente</strong><p>Estas pantallas aún no reciben datos de NGINX ni ejecutan HDBSCAN.</p></div></div>` +
    `<section class="page-heading"><div><p class="eyebrow">ANÁLISIS HTTP</p><h1>${title}</h1><p>${subtitle}</p></div></section>` +
    `<div class="http-context"><label>Unidad de análisis<select id="http-window"><option value="5">Ventanas de 5 minutos</option><option value="15">Ventanas de 15 minutos</option></select></label><p>El clúster se asignará a una ventana temporal, no a cada petición.</p></div>` + content;
