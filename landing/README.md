# Landing de MindLink

Landing estática de una sola página preparada para Netlify.

## Prueba local

Desde la raíz del repositorio:

```powershell
Start-Process .\landing\index.html
```

En modo local, los registros se guardan únicamente en `localStorage` para facilitar las pruebas.

## Publicación en Netlify

1. Inicia sesión en `https://app.netlify.com/`.
2. Abre `https://app.netlify.com/drop`.
3. Arrastra la carpeta `landing` completa.
4. Abre el sitio generado y realiza un registro de prueba.
5. En Netlify, abre **Forms** y habilita **Form detection** si todavía aparece desactivado.
6. Si fue necesario habilitarlo, vuelve a desplegar la carpeta.
7. Comprueba que aparezca el formulario `mindlink-waitlist` y el registro de prueba.

En producción, el formulario se envía mediante POST a Netlify Forms. Los registros dejan de depender del navegador del visitante.

## Notificaciones

En el sitio de Netlify abre:

`Project configuration > Notifications > Form submission notifications`

Agrega una notificación por correo para recibir cada registro de la lista de espera.

## Video de demostración

El video está incluido en `videos/mindlink-propuesta-valor.mp4`. Se reproduce automáticamente y sin sonido dentro de la sección principal. Incluye comentarios breves sincronizados cada cuatro segundos con cada pantalla, controles para pausar o activar el sonido y una vista ampliada al pulsar sobre el video o el botón **Ver demostración**.

Si se actualiza la grabación, reemplaza ese archivo conservando el mismo nombre para no cambiar la ruta de la landing.
