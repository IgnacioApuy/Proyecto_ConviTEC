// El frontend no tiene lógica de negocio: solo llama a la API y muestra el resultado.
async function cargarSede() {
  const textoSede = document.getElementById('sede');
  const textoEstado = document.getElementById('estado');

  try {
    const respuesta = await fetch('/api/sede');
    if (!respuesta.ok) throw new Error('HTTP ' + respuesta.status);

    const sede = await respuesta.json();
    textoSede.textContent = 'Sede ' + sede.nombre + ' (' + sede.codigo + ')';
    document.title = 'ConviTEC | ' + sede.nombre;
    textoEstado.textContent = 'Conectado a la API de la sede.';
  } catch (error) {
    textoSede.textContent = 'Sin conexión con la sede';
    textoEstado.textContent = 'No se pudo contactar la API: ' + error.message;
  }
}

cargarSede();