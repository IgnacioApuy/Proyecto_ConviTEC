import { obtener, enviar } from './api.js';
import { el, aviso, tabla, colones, fecha, insigniaEstado, listaDetalles } from './ui.js';

// ---------- Consultas ----------

async function vistaVentas(contenedor) {
  const ventas = await obtener('/api/ventas');
  contenedor.append(tabla([
    { titulo: 'Venta', valor: v => v.idVenta },
    { titulo: 'Pedido', valor: v => v.idPedido },
    { titulo: 'Fecha', valor: v => fecha(v.fecha) },
    { titulo: 'Productos', valor: v => listaDetalles(v.detalles) },
    { titulo: 'Monto total', clase: 'num', valor: v => colones(v.montoTotal) }
  ], ventas));
}

async function vistaPedidos(contenedor) {
  const pedidos = await obtener('/api/pedidos');
  contenedor.append(tabla([
    { titulo: 'Pedido', valor: p => p.id },
    { titulo: 'Creado', valor: p => fecha(p.fechaCreacion) },
    { titulo: 'Estado', valor: p => insigniaEstado(p.estado) },
    { titulo: 'Destino', valor: p => p.destino },
    { titulo: 'Sedes participantes', valor: p => p.sedesParticipantes.join(', ') },
    { titulo: 'Productos', valor: p => listaDetalles(p.detalles) },
    { titulo: 'Monto total', clase: 'num', valor: p => colones(p.montoTotal) }
  ], pedidos));
}

function columnasPendientes() {
  return [
    { titulo: 'Pedido', valor: p => p.id },
    { titulo: 'Creado', valor: p => fecha(p.fechaCreacion) },
    { titulo: 'Destino', valor: p => p.destino },
    { titulo: 'Productos', valor: p => listaDetalles(p.detalles) },
    { titulo: 'Monto total', clase: 'num', valor: p => colones(p.montoTotal) }
  ];
}

async function vistaPendientes(contenedor) {
  contenedor.append(tabla(columnasPendientes(), await obtener('/api/pedidos/pendientes')));
}

async function vistaInventario(contenedor) {
  const inventario = await obtener('/api/inventario');
  contenedor.append(tabla([
    { titulo: 'Código', valor: p => p.codigo },
    { titulo: 'Producto', valor: p => p.nombre },
    { titulo: 'Categoría', valor: p => p.categoria },
    { titulo: 'Precio', clase: 'num', valor: p => colones(p.precio) },
    { titulo: 'Cantidad', clase: 'num', valor: p => p.cantidad }
  ], inventario));
}

async function vistaCatalogo(contenedor) {
  const catalogo = await obtener('/api/catalogo');

  // Una columna por sede, tomadas de los datos y no escritas a mano.
  const sedes = catalogo.length > 0 ? catalogo[0].disponibilidad.map(d => d.sede) : [];

  const columnas = [
    { titulo: 'Código', valor: p => p.codigo },
    { titulo: 'Producto', valor: p => p.nombre },
    { titulo: 'Descripción', valor: p => p.descripcion },
    { titulo: 'Categoría', valor: p => p.categoria },
    { titulo: 'Precio', clase: 'num', valor: p => colones(p.precio) }
  ];

  for (const sede of sedes) {
    columnas.push({
      titulo: sede,
      clase: 'num',
      valor: p => p.disponibilidad.find(d => d.sede === sede)?.cantidad ?? 0
    });
  }

  contenedor.append(tabla(columnas, catalogo));
}

async function vistaProductoEstrella(contenedor) {
  const estrella = await obtener('/api/estadisticas/producto-estrella');

  if (!estrella.hayVentas) {
    contenedor.append(aviso(estrella.mensaje));
    return;
  }

  contenedor.append(el('div', { class: 'tarjeta' },
    el('h3', {}, estrella.nombre),
    el('p', { class: 'dato-grande' }, colones(estrella.ingresos)),
    el('p', {}, `${estrella.unidadesVendidas} unidades × ${colones(estrella.precio)}`),
    el('p', { class: 'secundario' }, `Código ${estrella.codigo}`)));
}

async function vistaPeoresPedidos(contenedor) {
  const pedidos = await obtener('/api/estadisticas/peores-pedidos');

  contenedor.append(el('p', { class: 'secundario' },
    'Rendimiento = monto de venta (₡) ÷ tiempo de traslado (min). Del más bajo al más alto.'));

  contenedor.append(tabla([
    { titulo: '#', valor: (p, i) => i + 1 },
    { titulo: 'Pedido', valor: p => p.id },
    { titulo: 'Destino', valor: p => p.destino },
    { titulo: 'Monto', clase: 'num', valor: p => colones(p.montoTotal) },
    { titulo: 'Traslado', clase: 'num', valor: p => `${p.tiempoTrasladoMin} min` },
    { titulo: 'Rendimiento', clase: 'num', valor: p => `${colones(p.rendimiento)} / min` }
  ], pedidos));
}

// ---------- Operaciones ----------

async function vistaRealizarPedido(contenedor) {
  const [catalogo, destinos] = await Promise.all([
    obtener('/api/catalogo'),
    obtener('/api/destinos')
  ]);

  const filas = el('div', {});
  const resultado = el('div', {});

  const selectDestino = el('select', { id: 'destino', required: '' },
    el('option', { value: '' }, 'Seleccione un destino'),
    destinos.map(d => el('option', { value: d }, d)));

  function crearFila() {
    const producto = el('select', { required: '', 'aria-label': 'Producto' },
      el('option', { value: '' }, 'Seleccione un producto'),
      catalogo.map(p => el('option', { value: p.codigo }, `${p.nombre} (${colones(p.precio)})`)));

    const cantidad = el('input', {
      type: 'number', min: '1', step: '1', value: '1', required: '', 'aria-label': 'Cantidad'
    });

    const fila = el('div', { class: 'fila-producto' },
      producto,
      cantidad,
      el('button', {
        type: 'button',
        class: 'secundario-btn',
        onclick: () => { if (filas.children.length > 1) fila.remove(); }
      }, 'Quitar'));

    return fila;
  }

  function leerItems() {
    return Array.from(filas.children).map(fila => ({
      codigo: fila.querySelector('select').value,
      cantidad: Number(fila.querySelector('input').value)
    }));
  }

  filas.append(crearFila());

  const botonEnviar = el('button', { type: 'submit' }, 'Realizar pedido');

  const formulario = el('form', {
    onsubmit: async (evento) => {
      evento.preventDefault();
      botonEnviar.disabled = true;
      resultado.replaceChildren();

      try {
        const r = await enviar('/api/pedidos', { destino: selectDestino.value, items: leerItems() });

        if (r.ok && r.datos?.aceptado) {
          resultado.append(aviso(`${r.datos.mensaje} Monto total: ${colones(r.datos.montoTotal)}.`, 'ok'));
          formulario.reset();
          filas.replaceChildren(crearFila());
        } else if (r.ok) {
          resultado.append(aviso(`Pedido rechazado: ${r.datos.motivo}`, 'aviso'));
        } else {
          resultado.append(aviso(r.datos?.error ?? `Error inesperado (código ${r.estado}).`, 'error'));
        }
      } catch {
        resultado.append(aviso('No se pudo contactar la API de la sede.', 'error'));
      } finally {
        botonEnviar.disabled = false;
      }
    }
  },
    el('label', { for: 'destino' }, 'Punto de entrega'),
    selectDestino,
    el('label', {}, 'Productos'),
    filas,
    el('button', {
      type: 'button',
      class: 'secundario-btn',
      onclick: () => filas.append(crearFila())
    }, '+ Agregar producto'),
    el('div', { class: 'acciones' }, botonEnviar),
    resultado);

  contenedor.append(formulario);
}

async function vistaEntregasPendientes(contenedor) {
  contenedor.append(tabla(columnasPendientes(), await obtener('/api/entregas/pendientes')));
}

async function vistaSiguienteEntrega(contenedor) {
  const salida = el('div', {});

  const boton = el('button', {
    onclick: async () => {
      boton.disabled = true;
      salida.replaceChildren();

      try {
        const r = await enviar('/api/entregas/siguiente');

        if (!r.ok) {
          salida.append(aviso(r.datos?.error ?? `Error inesperado (código ${r.estado}).`, 'error'));
        } else if (!r.datos.hayPendientes) {
          salida.append(aviso(r.datos.mensaje));
        } else {
          salida.append(tarjetaEntrega(r.datos));
        }
      } catch {
        salida.append(aviso('No se pudo contactar la API de la sede.', 'error'));
      } finally {
        boton.disabled = false;
      }
    }
  }, 'Calcular siguiente punto de entrega');

  contenedor.append(boton, salida);
}

function tarjetaEntrega(datos) {
  const ultima = datos.ruta.length - 1;
  const paradas = [];

  datos.ruta.forEach((lugar, i) => {
    if (i > 0) paradas.push(el('span', { class: 'flecha' }, '→'));
    const clase = i === 0 ? 'parada origen' : i === ultima ? 'parada destino' : 'parada';
    paradas.push(el('span', { class: clase }, lugar));
  });

  return el('div', { class: 'tarjeta' },
    el('h3', {}, `Siguiente entrega: ${datos.destino}`),
    el('p', {}, `Ubicación actual: ${datos.ubicacionActual}`),
    el('div', { class: 'ruta' }, paradas),
    el('p', {}, `Tiempo de traslado: ${datos.tiempoMinutos} min`),
    el('p', { class: 'secundario' }, `Pedidos a entregar: ${datos.pedidos.join(', ')}`));
}

// ---------- Menú ----------

export const vistas = [
  { id: 'ventas', grupo: 'Consultas', titulo: 'Historial de ventas', render: vistaVentas },
  { id: 'pedidos', grupo: 'Consultas', titulo: 'Historial de pedidos', render: vistaPedidos },
  { id: 'pendientes', grupo: 'Consultas', titulo: 'Pedidos pendientes', render: vistaPendientes },
  { id: 'inventario', grupo: 'Consultas', titulo: 'Inventario local', render: vistaInventario },
  { id: 'catalogo', grupo: 'Consultas', titulo: 'Catálogo global', render: vistaCatalogo },
  { id: 'estrella', grupo: 'Consultas', titulo: 'Producto estrella', render: vistaProductoEstrella },
  { id: 'peores', grupo: 'Consultas', titulo: 'Peores pedidos', render: vistaPeoresPedidos },
  { id: 'nuevo-pedido', grupo: 'Operaciones', titulo: 'Realizar pedido', render: vistaRealizarPedido },
  { id: 'entregas', grupo: 'Operaciones', titulo: 'Entregas pendientes', render: vistaEntregasPendientes },
  { id: 'siguiente', grupo: 'Operaciones', titulo: 'Siguiente punto de entrega', render: vistaSiguienteEntrega }
];