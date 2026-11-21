// Cliente de la API de la sede. No contiene lógica de negocio.

// Para consultas (GET): si la API responde con error, lanza una excepción.
export async function obtener(ruta) {
  const respuesta = await fetch(ruta);
  if (!respuesta.ok) {
    throw new Error(`La API respondió con el código ${respuesta.status} en ${ruta}`);
  }
  return respuesta.json();
}

// Para operaciones (POST): no lanza error con códigos 400, porque el motivo
// viene en el cuerpo de la respuesta y se le muestra al usuario.
export async function enviar(ruta, cuerpo) {
  const opciones = { method: 'POST' };
  if (cuerpo !== undefined) {
    opciones.headers = { 'Content-Type': 'application/json' };
    opciones.body = JSON.stringify(cuerpo);
  }

  const respuesta = await fetch(ruta, opciones);

  let datos = null;
  try {
    datos = await respuesta.json();
  } catch {
    // La respuesta no traía JSON.
  }

  return { ok: respuesta.ok, estado: respuesta.status, datos };
}