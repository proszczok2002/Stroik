# Stroik 1.20 — Guía

Afinador de Windows accesible para músicos ciegos, con baja visión y videntes.
Idioma y tema oscuro se eligen en Ajustes. Idiomas: Polski, English, Deutsch,
Русский, Français, Español. La elección se conserva en los perfiles JSON.

## Métodos

- Diapasón: referencia para afinar de oído, manual o automática.
- Electrónico: active el micrófono, toque una nota y apáguela.
  El resultado indica dirección y desviación.
- Híbrido con altavoces: referencia y micrófono alternan. Tras el resultado,
  espera a que termine la voz y una pausa ajustable de 3–60 segundos (8 por defecto).
  En la vista para principiantes, los consejos se leen antes de la referencia.
- Híbrido con auriculares: referencia y micrófono simultáneos. Active la
  monitorización solo con auriculares para evitar la realimentación acústica.

## Uso

Alt+N o Ctrl+N selecciona la siguiente nota del plan; con Mayús, la anterior.
En la lista, arriba/abajo eligen teclas blancas e izquierda/derecha solo negras.
I/S eligen notas alteradas. Alt+U da un golpe en modo manual o pausa/reanuda el
automático. Los campos numéricos aceptan punto y coma; arriba/abajo cambia su valor.
Ctrl+S guarda un perfil, Ctrl+Q cierra. F1 abre consejos accesibles por teclado.
La vista clásica es breve; la de principiantes incluye explicaciones y posición
de las teclas.

En modo diapasón, solo la casilla de dos golpes controla el micrófono. Primero
se necesita silencio; la referencia no cuenta como gesto. Con esta opción,
el intervalo automático mínimo es 6,5 segundos. NVDA 2024.1 o posterior informa
del fin de los consejos iniciales; el estado de voz requiere NVDA 2026.3.
Otros lectores disponen de un botón para empezar después de los consejos.
Biblioteca LGPL: THIRD_PARTY_NOTICES.md.

## Instrumentos y medición

Piano, guitarra, ukelele, violín, bajo y modo cromático. Rango de piano A0–C8;
objetivo inicial sol4 según la numeración científica. Quinta y octava se forman
hacia arriba: do4+sol4 o do4+do5 en esa numeración. La medición simultánea es
experimental: un armónico puede simular una segunda nota. Compruebe también
por separado. Las quintas temperadas pueden producir batimientos; el piano
puede necesitar octavas estiradas.

Los golpes demasiado débiles o fuertes falsean las lecturas. Toque uniformemente,
evite la saturación y ajuste la sensibilidad. 440 Hz es el estándar; consulte
a un técnico antes de aumentar la tensión de un instrumento antiguo. Corrija
el micrófono solo con una referencia conocida. El audio no se guarda. Los
armónicos ayudan a las notas graves; los altavoces pequeños siguen limitados.
Rojo indica desafinado y verde dentro de tolerancia; el texto sigue disponible.

## Perfiles, actualizaciones y proyecto

JSON guarda método, instrumento, nota, idioma, tema y opciones de actualización.
Puede desactivar la comprobación automática y ajustar su intervalo. Las versiones
nuevas muestran los cambios; la descarga se verifica con SHA-256. Se sustituye
el EXE y se reinicia el programa. Guárdelo en una carpeta con permiso de escritura.

Creado por Proszczok con Codex. Idea original: Witold. Licencia MIT.

https://github.com/proszczok2002/Stroik · https://proszczok.pl/

Apoyo: https://paypal.me/proszczok · https://revolut.me/r_rosiejka

[Android](android/README.es.md) · [English](README.en.md) · [Polski](README.md)
