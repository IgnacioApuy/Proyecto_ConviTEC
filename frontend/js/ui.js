// Utilidades para construir la interfaz. Siempre usan textContent, nunca innerHTML con datos.

export function el(etiqueta, atributos = {}, ...hijos) {
  const nodo = document.createElement(etiqueta);

  for (const [clave, valor] of Object.entries(atributos)) {
    if (clave === 'class') nodo.className = valor;
    else if (clave.startsWith('on')) nodo.addEventListener(clave.slice(2), valor);
    else nodo.setAttribute(clave, valor);
  }

  for (const hijo of hijos.flat()) {
    if (hijo === null || hijo === undefined) continue;
    nodo.append(hijo instanceof Node ? hijo : document.createTextNode(String(hijo)));
  }

  return nodo;
}

// tipo: 'info' | 'ok' | 'aviso' | 'error'
export function aviso(texto, tipo = 'info') {
  return el('div', { class: `aviso aviso-${tipo}`, role: 'status' }, texto);
}

// columnas: [{ titulo, valor: (fila, indice) => texto o nodo, clase? }]
export function tabla(columnas, filas) {
  if (filas.length === 0) return aviso('No hay registros para mostrar.');

  const encabezado = el('thead', {}, el('tr', {},
    columnas.map(c => el('th', { class: c.clase ?? '' }, c.titulo))));

  const cuerpo = el('tbody', {}, filas.map((fila, i) =>
    el('tr', {}, columnas.map(c => el('td', { class: c.clase ?? '' }, c.valor(fila, i))))));

  return el('div', { class: 'tabla-contenedor' }, el('table', {}, encabezado, cuerpo));
}

const formatoMoneda = new Intl.NumberFormat('es-CR', {
  style: 'currency',
  currency: 'CRC',
  maximumFractionDigits: 0
});

export function colones(monto) {
  return formatoMoneda.format(monto);
}

export function fecha(texto) {
  if (!texto) return '—';
  return new Date(texto).toLocaleString('es-CR', { dateStyle: 'short', timeStyle: 'short' });
}

export function insigniaEstado(estado) {
  return el('span', { class: `estado estado-${estado.toLowerCase()}` }, estado);
}

// Lista de productos de un pedido o venta: "3 × Agua 600 ml".
export function listaDetalles(detalles) {
  return el('ul', {}, detalles.map(d => el('li', {}, `${d.cantidad} × ${d.nombre}`)));
}