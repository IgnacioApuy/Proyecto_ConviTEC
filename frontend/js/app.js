import { obtener } from './api.js';
import { el, aviso } from './ui.js';
import { vistas } from './vistas.js';

const menu = document.getElementById('menu');
const contenido = document.getElementById('contenido');

async function cargarSede() {
  const textoSede = document.getElementById('sede');

  try {
    const sede = await obtener('/api/sede');
    textoSede.textContent = `Sede ${sede.nombre} (${sede.codigo})`;
    document.title = `ConviTEC | ${sede.nombre}`;
  } catch {
    textoSede.textContent = 'Sin conexión con la sede';
  }
}

function construirMenu() {
  let grupoActual = null;

  for (const vista of vistas) {
    if (vista.grupo !== grupoActual) {
      grupoActual = vista.grupo;
      menu.append(el('h3', {}, grupoActual));
    }
    menu.append(el('a', { href: `#${vista.id}` }, vista.titulo));
  }
}

async function mostrarVista() {
  const id = location.hash.slice(1) || vistas[0].id;
  const vista = vistas.find(v => v.id === id) ?? vistas[0];

  for (const enlace of menu.querySelectorAll('a')) {
    enlace.classList.toggle('activo', enlace.getAttribute('href') === `#${vista.id}`);
  }

  const cuerpo = el('div', {}, aviso('Cargando...'));
  contenido.replaceChildren(el('h2', {}, vista.titulo), cuerpo);

  const contenedor = el('div', {});
  try {
    await vista.render(contenedor);
    cuerpo.replaceChildren(contenedor);
  } catch (error) {
    cuerpo.replaceChildren(aviso(`No se pudo cargar la información: ${error.message}`, 'error'));
  }
}

construirMenu();
cargarSede();
window.addEventListener('hashchange', mostrarVista);
mostrarVista();