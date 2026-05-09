using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations;

/// <summary>
/// Seeds the description for the DB_FRAMEBUFFER sentinel GPU
/// (id = -2, see <c>Marechai.Database.Operations.DbSoftware</c> — the
/// constant name is historical; the GPU row stores the framebuffer
/// concept) in all five languages supported by the application UI:
/// English (eng), Spanish (spa), German (deu), French (fra) and
/// Italian (ita).
///
/// The DB_FRAMEBUFFER row itself is seeded by the legacy pre-EF
/// migration <c>UpdateDatabaseToV12</c> in
/// <c>Marechai.Database/Operations/Update.cs</c>. This migration only
/// fills the `GpuDescriptions` table for it.
///
/// Inserts are idempotent (gated by `WHERE NOT EXISTS`) so re-running
/// the migration on a DB where some or all rows already exist will not
/// fail with a duplicate-key error.
/// </summary>
public partial class SeedDbFramebufferGpuDescriptions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // -------- eng --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'eng', '# Framebuffer-Based Graphics
        ### When the CPU Draws Every Pixel

        A computer that ""uses a framebuffer"" relies on one of the most fundamental approaches to digital graphics: **the software draws directly into a block of memory whose contents map one-to-one to the pixels on the screen**. This method predates modern GPUs and remains foundational to how displays operate even today.

        ## What a Framebuffer Is

        A **framebuffer** is a reserved region of memory—either carved out of main system RAM or implemented as a dedicated video memory bank—where each byte or group of bytes corresponds to a pixel or group of pixels. When software writes color values into this memory, it is effectively painting the screen.

        Unlike systems with a graphics processing unit, a framebuffer machine has **no hardware assistance** for:

        - Geometry or polygon processing
        - Sprite handling
        - Blitting or scaling
        - Color blending or shading

        All of these tasks fall entirely on the **main processor**, which must compute the final pixel values and store them directly into the framebuffer.

        ## The Role of the RAMDAC

        To turn the framebuffer''s digital contents into a visible image, the memory is connected to a chip commonly known as a **RAMDAC** (Random Access Memory Digital-to-Analog Converter). The RAMDAC:

        - Reads pixel data from the framebuffer
        - Converts those values into analog (or later, digital) signals
        - Sends the resulting signal to the display

        Crucially, the RAMDAC performs **no graphics processing**. It does not draw shapes, move sprites, or accelerate rendering. It simply translates the framebuffer''s raw pixel data into the electrical format required by the monitor.

        ## Strengths and Weaknesses

        ### Advantages

        - **Unlimited flexibility:** Any image the CPU can compute can be displayed—there are no hardware-imposed sprite limits, tile maps, or fixed color modes.
        - **Predictable behavior:** The framebuffer is a simple, transparent model that developers can manipulate directly.
        - **Universality:** Many operating systems and graphical environments are built around the framebuffer concept.

        ### Limitations

        - **High CPU load:** All rendering work competes with the main application or game logic.
        - **Performance bottlenecks:** As resolutions and color depths increase, the CPU may struggle to update the framebuffer fast enough.
        - **No hardware acceleration:** Effects such as scrolling, scaling, or animation must be implemented entirely in software.

        ## Historical Use

        Framebuffer-based graphics appear throughout computing history, from early workstations to consumer systems. Importantly, this approach was not limited to inexpensive machines. Designers sometimes omitted a dedicated GPU simply because:

        - The main processor was believed to be powerful enough
        - The system''s intended use did not require advanced graphics
        - The simplicity of a framebuffer architecture was desirable

        In practice, many such systems eventually revealed the limits of relying solely on the CPU for rendering, especially as graphical expectations grew.

        ## Technical Summary

        - **Definition:** A memory region whose contents directly represent on-screen pixels.
        - **Rendering Model:** CPU computes and writes all pixel data.
        - **Output Hardware:** RAMDAC converts framebuffer contents to display signals.
        - **CPU Load:** Significant; increases with resolution and color depth.
        - **Graphics Capability:** Limited only by CPU power and memory bandwidth.
        - **Historical Role:** Used in systems ranging from early microcomputers to high-end workstations.
        - **Superseded By:** GPUs and integrated graphics processors with hardware acceleration.
        ', '<h1>Framebuffer-Based Graphics</h1>
        <h3>When the CPU Draws Every Pixel</h3>
        <p>A computer that ""uses a framebuffer"" relies on one of the most fundamental approaches to digital graphics: <strong>the software draws directly into a block of memory whose contents map one-to-one to the pixels on the screen</strong>. This method predates modern GPUs and remains foundational to how displays operate even today.</p>
        <h2>What a Framebuffer Is</h2>
        <p>A <strong>framebuffer</strong> is a reserved region of memory—either carved out of main system RAM or implemented as a dedicated video memory bank—where each byte or group of bytes corresponds to a pixel or group of pixels. When software writes color values into this memory, it is effectively painting the screen.</p>
        <p>Unlike systems with a graphics processing unit, a framebuffer machine has <strong>no hardware assistance</strong> for:</p>
        <ul><li>Geometry or polygon processing</li><li>Sprite handling</li><li>Blitting or scaling</li><li>Color blending or shading</li></ul>
        <p>All of these tasks fall entirely on the <strong>main processor</strong>, which must compute the final pixel values and store them directly into the framebuffer.</p>
        <h2>The Role of the RAMDAC</h2>
        <p>To turn the framebuffer''s digital contents into a visible image, the memory is connected to a chip commonly known as a <strong>RAMDAC</strong> (Random Access Memory Digital-to-Analog Converter). The RAMDAC:</p>
        <ul><li>Reads pixel data from the framebuffer</li><li>Converts those values into analog (or later, digital) signals</li><li>Sends the resulting signal to the display</li></ul>
        <p>Crucially, the RAMDAC performs <strong>no graphics processing</strong>. It does not draw shapes, move sprites, or accelerate rendering. It simply translates the framebuffer''s raw pixel data into the electrical format required by the monitor.</p>
        <h2>Strengths and Weaknesses</h2>
        <h3>Advantages</h3>
        <ul><li><strong>Unlimited flexibility:</strong> Any image the CPU can compute can be displayed—there are no hardware-imposed sprite limits, tile maps, or fixed color modes.</li><li><strong>Predictable behavior:</strong> The framebuffer is a simple, transparent model that developers can manipulate directly.</li><li><strong>Universality:</strong> Many operating systems and graphical environments are built around the framebuffer concept.</li></ul>
        <h3>Limitations</h3>
        <ul><li><strong>High CPU load:</strong> All rendering work competes with the main application or game logic.</li><li><strong>Performance bottlenecks:</strong> As resolutions and color depths increase, the CPU may struggle to update the framebuffer fast enough.</li><li><strong>No hardware acceleration:</strong> Effects such as scrolling, scaling, or animation must be implemented entirely in software.</li></ul>
        <h2>Historical Use</h2>
        <p>Framebuffer-based graphics appear throughout computing history, from early workstations to consumer systems. Importantly, this approach was not limited to inexpensive machines. Designers sometimes omitted a dedicated GPU simply because:</p>
        <ul><li>The main processor was believed to be powerful enough</li><li>The system''s intended use did not require advanced graphics</li><li>The simplicity of a framebuffer architecture was desirable</li></ul>
        <p>In practice, many such systems eventually revealed the limits of relying solely on the CPU for rendering, especially as graphical expectations grew.</p>
        <h2>Technical Summary</h2>
        <ul><li><strong>Definition:</strong> A memory region whose contents directly represent on-screen pixels.</li><li><strong>Rendering Model:</strong> CPU computes and writes all pixel data.</li><li><strong>Output Hardware:</strong> RAMDAC converts framebuffer contents to display signals.</li><li><strong>CPU Load:</strong> Significant; increases with resolution and color depth.</li><li><strong>Graphics Capability:</strong> Limited only by CPU power and memory bandwidth.</li><li><strong>Historical Role:</strong> Used in systems ranging from early microcomputers to high-end workstations.</li><li><strong>Superseded By:</strong> GPUs and integrated graphics processors with hardware acceleration.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -2 AND `LanguageCode` = 'eng'
            );
        ");

        // -------- spa --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'spa', '# Gráficos basados en framebuffer
        ### Cuando la CPU dibuja cada píxel

        Un ordenador que ""usa un framebuffer"" se apoya en uno de los enfoques más fundamentales de los gráficos digitales: **el software dibuja directamente sobre un bloque de memoria cuyo contenido se corresponde uno a uno con los píxeles de la pantalla**. Este método es anterior a las GPU modernas y sigue siendo la base sobre la que funcionan las pantallas hoy en día.

        ## Qué es un framebuffer

        Un **framebuffer** es una región reservada de memoria —tomada de la RAM principal del sistema o implementada como un banco de memoria de vídeo dedicado— en la que cada byte o grupo de bytes corresponde a un píxel o a un grupo de píxeles. Cuando el software escribe valores de color en esa memoria, en realidad está pintando la pantalla.

        A diferencia de los sistemas con una unidad de procesamiento gráfico, una máquina con framebuffer no dispone de **ninguna ayuda por hardware** para:

        - Procesamiento de geometría o polígonos
        - Gestión de sprites
        - Blitting o escalado
        - Mezcla de colores o sombreado

        Todas estas tareas recaen íntegramente en el **procesador principal**, que debe calcular los valores finales de los píxeles y almacenarlos directamente en el framebuffer.

        ## El papel del RAMDAC

        Para convertir el contenido digital del framebuffer en una imagen visible, la memoria se conecta a un chip comúnmente conocido como **RAMDAC** (Random Access Memory Digital-to-Analog Converter, conversor de memoria de acceso aleatorio digital a analógico). El RAMDAC:

        - Lee los datos de píxel del framebuffer
        - Convierte esos valores en señales analógicas (o, más adelante, digitales)
        - Envía la señal resultante a la pantalla

        Es importante destacar que el RAMDAC no realiza **ningún tipo de procesamiento gráfico**. No dibuja formas, no mueve sprites ni acelera el renderizado. Simplemente traduce los datos de píxel en bruto del framebuffer al formato eléctrico que requiere el monitor.

        ## Fortalezas y debilidades

        ### Ventajas

        - **Flexibilidad ilimitada:** Puede mostrarse cualquier imagen que la CPU sea capaz de calcular: no hay límites de sprites impuestos por hardware, ni mapas de tiles, ni modos de color fijos.
        - **Comportamiento predecible:** El framebuffer es un modelo sencillo y transparente que los desarrolladores pueden manipular directamente.
        - **Universalidad:** Muchos sistemas operativos y entornos gráficos se construyen en torno al concepto de framebuffer.

        ### Limitaciones

        - **Alta carga de CPU:** Todo el trabajo de renderizado compite con la lógica de la aplicación o el juego principal.
        - **Cuellos de botella de rendimiento:** A medida que aumentan las resoluciones y la profundidad de color, la CPU puede no llegar a actualizar el framebuffer con suficiente rapidez.
        - **Sin aceleración por hardware:** Efectos como el desplazamiento, el escalado o la animación deben implementarse íntegramente por software.

        ## Uso histórico

        Los gráficos basados en framebuffer aparecen a lo largo de toda la historia de la informática, desde las primeras estaciones de trabajo hasta los sistemas de consumo. Cabe destacar que este enfoque no se limitó a máquinas baratas. Los diseñadores omitían a veces una GPU dedicada simplemente porque:

        - Se consideraba que el procesador principal era suficientemente potente
        - El uso previsto del sistema no requería gráficos avanzados
        - Resultaba deseable la sencillez de una arquitectura de framebuffer

        En la práctica, muchos de esos sistemas acabaron mostrando las limitaciones de depender únicamente de la CPU para el renderizado, sobre todo a medida que crecían las expectativas gráficas.

        ## Resumen técnico

        - **Definición:** Región de memoria cuyo contenido representa directamente los píxeles en pantalla.
        - **Modelo de renderizado:** La CPU calcula y escribe todos los datos de píxel.
        - **Hardware de salida:** El RAMDAC convierte el contenido del framebuffer en señales para la pantalla.
        - **Carga de CPU:** Significativa; aumenta con la resolución y la profundidad de color.
        - **Capacidad gráfica:** Limitada únicamente por la potencia de la CPU y el ancho de banda de memoria.
        - **Papel histórico:** Empleado en sistemas que van desde primeros microordenadores hasta estaciones de trabajo de alta gama.
        - **Sustituido por:** GPU y procesadores gráficos integrados con aceleración por hardware.
        ', '<h1>Gráficos basados en framebuffer</h1>
        <h3>Cuando la CPU dibuja cada píxel</h3>
        <p>Un ordenador que ""usa un framebuffer"" se apoya en uno de los enfoques más fundamentales de los gráficos digitales: <strong>el software dibuja directamente sobre un bloque de memoria cuyo contenido se corresponde uno a uno con los píxeles de la pantalla</strong>. Este método es anterior a las GPU modernas y sigue siendo la base sobre la que funcionan las pantallas hoy en día.</p>
        <h2>Qué es un framebuffer</h2>
        <p>Un <strong>framebuffer</strong> es una región reservada de memoria —tomada de la RAM principal del sistema o implementada como un banco de memoria de vídeo dedicado— en la que cada byte o grupo de bytes corresponde a un píxel o a un grupo de píxeles. Cuando el software escribe valores de color en esa memoria, en realidad está pintando la pantalla.</p>
        <p>A diferencia de los sistemas con una unidad de procesamiento gráfico, una máquina con framebuffer no dispone de <strong>ninguna ayuda por hardware</strong> para:</p>
        <ul><li>Procesamiento de geometría o polígonos</li><li>Gestión de sprites</li><li>Blitting o escalado</li><li>Mezcla de colores o sombreado</li></ul>
        <p>Todas estas tareas recaen íntegramente en el <strong>procesador principal</strong>, que debe calcular los valores finales de los píxeles y almacenarlos directamente en el framebuffer.</p>
        <h2>El papel del RAMDAC</h2>
        <p>Para convertir el contenido digital del framebuffer en una imagen visible, la memoria se conecta a un chip comúnmente conocido como <strong>RAMDAC</strong> (Random Access Memory Digital-to-Analog Converter, conversor de memoria de acceso aleatorio digital a analógico). El RAMDAC:</p>
        <ul><li>Lee los datos de píxel del framebuffer</li><li>Convierte esos valores en señales analógicas (o, más adelante, digitales)</li><li>Envía la señal resultante a la pantalla</li></ul>
        <p>Es importante destacar que el RAMDAC no realiza <strong>ningún tipo de procesamiento gráfico</strong>. No dibuja formas, no mueve sprites ni acelera el renderizado. Simplemente traduce los datos de píxel en bruto del framebuffer al formato eléctrico que requiere el monitor.</p>
        <h2>Fortalezas y debilidades</h2>
        <h3>Ventajas</h3>
        <ul><li><strong>Flexibilidad ilimitada:</strong> Puede mostrarse cualquier imagen que la CPU sea capaz de calcular: no hay límites de sprites impuestos por hardware, ni mapas de tiles, ni modos de color fijos.</li><li><strong>Comportamiento predecible:</strong> El framebuffer es un modelo sencillo y transparente que los desarrolladores pueden manipular directamente.</li><li><strong>Universalidad:</strong> Muchos sistemas operativos y entornos gráficos se construyen en torno al concepto de framebuffer.</li></ul>
        <h3>Limitaciones</h3>
        <ul><li><strong>Alta carga de CPU:</strong> Todo el trabajo de renderizado compite con la lógica de la aplicación o el juego principal.</li><li><strong>Cuellos de botella de rendimiento:</strong> A medida que aumentan las resoluciones y la profundidad de color, la CPU puede no llegar a actualizar el framebuffer con suficiente rapidez.</li><li><strong>Sin aceleración por hardware:</strong> Efectos como el desplazamiento, el escalado o la animación deben implementarse íntegramente por software.</li></ul>
        <h2>Uso histórico</h2>
        <p>Los gráficos basados en framebuffer aparecen a lo largo de toda la historia de la informática, desde las primeras estaciones de trabajo hasta los sistemas de consumo. Cabe destacar que este enfoque no se limitó a máquinas baratas. Los diseñadores omitían a veces una GPU dedicada simplemente porque:</p>
        <ul><li>Se consideraba que el procesador principal era suficientemente potente</li><li>El uso previsto del sistema no requería gráficos avanzados</li><li>Resultaba deseable la sencillez de una arquitectura de framebuffer</li></ul>
        <p>En la práctica, muchos de esos sistemas acabaron mostrando las limitaciones de depender únicamente de la CPU para el renderizado, sobre todo a medida que crecían las expectativas gráficas.</p>
        <h2>Resumen técnico</h2>
        <ul><li><strong>Definición:</strong> Región de memoria cuyo contenido representa directamente los píxeles en pantalla.</li><li><strong>Modelo de renderizado:</strong> La CPU calcula y escribe todos los datos de píxel.</li><li><strong>Hardware de salida:</strong> El RAMDAC convierte el contenido del framebuffer en señales para la pantalla.</li><li><strong>Carga de CPU:</strong> Significativa; aumenta con la resolución y la profundidad de color.</li><li><strong>Capacidad gráfica:</strong> Limitada únicamente por la potencia de la CPU y el ancho de banda de memoria.</li><li><strong>Papel histórico:</strong> Empleado en sistemas que van desde primeros microordenadores hasta estaciones de trabajo de alta gama.</li><li><strong>Sustituido por:</strong> GPU y procesadores gráficos integrados con aceleración por hardware.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -2 AND `LanguageCode` = 'spa'
            );
        ");

        // -------- deu --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'deu', '# Framebuffer-basierte Grafik
        ### Wenn die CPU jeden Pixel zeichnet

        Ein Computer, der ""einen Framebuffer verwendet"", stützt sich auf einen der grundlegendsten Ansätze der digitalen Grafik: **die Software zeichnet direkt in einen Speicherblock, dessen Inhalt eins zu eins den Pixeln auf dem Bildschirm entspricht**. Diese Methode ist älter als moderne GPUs und bildet bis heute eine grundlegende Funktionsweise von Anzeigen.

        ## Was ein Framebuffer ist

        Ein **Framebuffer** ist ein reservierter Speicherbereich — entweder aus dem Haupt-RAM des Systems abgetrennt oder als dedizierte Videospeicherbank realisiert —, in dem jedes Byte oder jede Bytegruppe einem Pixel oder einer Pixelgruppe entspricht. Wenn die Software Farbwerte in diesen Speicher schreibt, malt sie damit faktisch den Bildschirm.

        Im Gegensatz zu Systemen mit einer Grafikverarbeitungseinheit hat eine Framebuffer-Maschine **keine Hardwareunterstützung** für:

        - Geometrie- oder Polygonverarbeitung
        - Sprite-Verwaltung
        - Blitting oder Skalierung
        - Farbmischung oder Shading

        All diese Aufgaben fallen vollständig dem **Hauptprozessor** zu, der die endgültigen Pixelwerte berechnen und direkt in den Framebuffer schreiben muss.

        ## Die Rolle des RAMDAC

        Um den digitalen Inhalt des Framebuffers in ein sichtbares Bild zu verwandeln, ist der Speicher an einen Chip angeschlossen, der gemeinhin als **RAMDAC** (Random Access Memory Digital-to-Analog Converter) bezeichnet wird. Der RAMDAC:

        - liest die Pixeldaten aus dem Framebuffer
        - wandelt diese Werte in analoge (später auch digitale) Signale um
        - sendet das resultierende Signal an die Anzeige

        Entscheidend ist: Der RAMDAC führt **keinerlei Grafikverarbeitung** durch. Er zeichnet keine Formen, bewegt keine Sprites und beschleunigt kein Rendering. Er übersetzt lediglich die rohen Pixeldaten des Framebuffers in das elektrische Format, das der Monitor benötigt.

        ## Stärken und Schwächen

        ### Vorteile

        - **Unbegrenzte Flexibilität:** Jedes Bild, das die CPU berechnen kann, lässt sich anzeigen — es gibt keine hardwarebedingten Sprite-Limits, keine Tile-Maps und keine festen Farbmodi.
        - **Vorhersagbares Verhalten:** Der Framebuffer ist ein einfaches, transparentes Modell, das Entwickler direkt manipulieren können.
        - **Universalität:** Viele Betriebssysteme und grafische Umgebungen sind um das Framebuffer-Konzept herum aufgebaut.

        ### Grenzen

        - **Hohe CPU-Last:** Sämtliche Rendering-Arbeit konkurriert mit der Logik der eigentlichen Anwendung oder des Spiels.
        - **Leistungsengpässe:** Mit steigender Auflösung und Farbtiefe kann es der CPU schwerfallen, den Framebuffer schnell genug zu aktualisieren.
        - **Keine Hardwarebeschleunigung:** Effekte wie Scrollen, Skalieren oder Animationen müssen vollständig in Software implementiert werden.

        ## Historische Verwendung

        Framebuffer-basierte Grafik durchzieht die gesamte Computergeschichte, von frühen Workstations bis zu Consumer-Systemen. Bemerkenswert ist, dass dieser Ansatz nicht auf günstige Maschinen beschränkt war. Konstrukteure verzichteten manchmal auf eine dedizierte GPU, einfach weil:

        - der Hauptprozessor als ausreichend leistungsfähig galt
        - der vorgesehene Einsatzzweck keine fortgeschrittene Grafik erforderte
        - die Einfachheit einer Framebuffer-Architektur erwünscht war

        In der Praxis offenbarten viele solcher Systeme schließlich die Grenzen des Renderings allein durch die CPU, insbesondere als die grafischen Erwartungen wuchsen.

        ## Technische Zusammenfassung

        - **Definition:** Ein Speicherbereich, dessen Inhalt direkt die Pixel auf dem Bildschirm repräsentiert.
        - **Rendering-Modell:** Die CPU berechnet und schreibt alle Pixeldaten.
        - **Ausgabehardware:** Der RAMDAC wandelt den Framebuffer-Inhalt in Anzeigesignale um.
        - **CPU-Last:** Erheblich; steigt mit Auflösung und Farbtiefe.
        - **Grafikfähigkeit:** Nur durch CPU-Leistung und Speicherbandbreite begrenzt.
        - **Historische Rolle:** Eingesetzt in Systemen vom frühen Mikrocomputer bis zur High-End-Workstation.
        - **Abgelöst durch:** GPUs und integrierte Grafikprozessoren mit Hardwarebeschleunigung.
        ', '<h1>Framebuffer-basierte Grafik</h1>
        <h3>Wenn die CPU jeden Pixel zeichnet</h3>
        <p>Ein Computer, der ""einen Framebuffer verwendet"", stützt sich auf einen der grundlegendsten Ansätze der digitalen Grafik: <strong>die Software zeichnet direkt in einen Speicherblock, dessen Inhalt eins zu eins den Pixeln auf dem Bildschirm entspricht</strong>. Diese Methode ist älter als moderne GPUs und bildet bis heute eine grundlegende Funktionsweise von Anzeigen.</p>
        <h2>Was ein Framebuffer ist</h2>
        <p>Ein <strong>Framebuffer</strong> ist ein reservierter Speicherbereich — entweder aus dem Haupt-RAM des Systems abgetrennt oder als dedizierte Videospeicherbank realisiert —, in dem jedes Byte oder jede Bytegruppe einem Pixel oder einer Pixelgruppe entspricht. Wenn die Software Farbwerte in diesen Speicher schreibt, malt sie damit faktisch den Bildschirm.</p>
        <p>Im Gegensatz zu Systemen mit einer Grafikverarbeitungseinheit hat eine Framebuffer-Maschine <strong>keine Hardwareunterstützung</strong> für:</p>
        <ul><li>Geometrie- oder Polygonverarbeitung</li><li>Sprite-Verwaltung</li><li>Blitting oder Skalierung</li><li>Farbmischung oder Shading</li></ul>
        <p>All diese Aufgaben fallen vollständig dem <strong>Hauptprozessor</strong> zu, der die endgültigen Pixelwerte berechnen und direkt in den Framebuffer schreiben muss.</p>
        <h2>Die Rolle des RAMDAC</h2>
        <p>Um den digitalen Inhalt des Framebuffers in ein sichtbares Bild zu verwandeln, ist der Speicher an einen Chip angeschlossen, der gemeinhin als <strong>RAMDAC</strong> (Random Access Memory Digital-to-Analog Converter) bezeichnet wird. Der RAMDAC:</p>
        <ul><li>liest die Pixeldaten aus dem Framebuffer</li><li>wandelt diese Werte in analoge (später auch digitale) Signale um</li><li>sendet das resultierende Signal an die Anzeige</li></ul>
        <p>Entscheidend ist: Der RAMDAC führt <strong>keinerlei Grafikverarbeitung</strong> durch. Er zeichnet keine Formen, bewegt keine Sprites und beschleunigt kein Rendering. Er übersetzt lediglich die rohen Pixeldaten des Framebuffers in das elektrische Format, das der Monitor benötigt.</p>
        <h2>Stärken und Schwächen</h2>
        <h3>Vorteile</h3>
        <ul><li><strong>Unbegrenzte Flexibilität:</strong> Jedes Bild, das die CPU berechnen kann, lässt sich anzeigen — es gibt keine hardwarebedingten Sprite-Limits, keine Tile-Maps und keine festen Farbmodi.</li><li><strong>Vorhersagbares Verhalten:</strong> Der Framebuffer ist ein einfaches, transparentes Modell, das Entwickler direkt manipulieren können.</li><li><strong>Universalität:</strong> Viele Betriebssysteme und grafische Umgebungen sind um das Framebuffer-Konzept herum aufgebaut.</li></ul>
        <h3>Grenzen</h3>
        <ul><li><strong>Hohe CPU-Last:</strong> Sämtliche Rendering-Arbeit konkurriert mit der Logik der eigentlichen Anwendung oder des Spiels.</li><li><strong>Leistungsengpässe:</strong> Mit steigender Auflösung und Farbtiefe kann es der CPU schwerfallen, den Framebuffer schnell genug zu aktualisieren.</li><li><strong>Keine Hardwarebeschleunigung:</strong> Effekte wie Scrollen, Skalieren oder Animationen müssen vollständig in Software implementiert werden.</li></ul>
        <h2>Historische Verwendung</h2>
        <p>Framebuffer-basierte Grafik durchzieht die gesamte Computergeschichte, von frühen Workstations bis zu Consumer-Systemen. Bemerkenswert ist, dass dieser Ansatz nicht auf günstige Maschinen beschränkt war. Konstrukteure verzichteten manchmal auf eine dedizierte GPU, einfach weil:</p>
        <ul><li>der Hauptprozessor als ausreichend leistungsfähig galt</li><li>der vorgesehene Einsatzzweck keine fortgeschrittene Grafik erforderte</li><li>die Einfachheit einer Framebuffer-Architektur erwünscht war</li></ul>
        <p>In der Praxis offenbarten viele solcher Systeme schließlich die Grenzen des Renderings allein durch die CPU, insbesondere als die grafischen Erwartungen wuchsen.</p>
        <h2>Technische Zusammenfassung</h2>
        <ul><li><strong>Definition:</strong> Ein Speicherbereich, dessen Inhalt direkt die Pixel auf dem Bildschirm repräsentiert.</li><li><strong>Rendering-Modell:</strong> Die CPU berechnet und schreibt alle Pixeldaten.</li><li><strong>Ausgabehardware:</strong> Der RAMDAC wandelt den Framebuffer-Inhalt in Anzeigesignale um.</li><li><strong>CPU-Last:</strong> Erheblich; steigt mit Auflösung und Farbtiefe.</li><li><strong>Grafikfähigkeit:</strong> Nur durch CPU-Leistung und Speicherbandbreite begrenzt.</li><li><strong>Historische Rolle:</strong> Eingesetzt in Systemen vom frühen Mikrocomputer bis zur High-End-Workstation.</li><li><strong>Abgelöst durch:</strong> GPUs und integrierte Grafikprozessoren mit Hardwarebeschleunigung.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -2 AND `LanguageCode` = 'deu'
            );
        ");

        // -------- fra --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'fra', '# Graphisme à base de framebuffer
        ### Quand le CPU dessine chaque pixel

        Un ordinateur qui « utilise un framebuffer » s''appuie sur l''une des approches les plus fondamentales du graphisme numérique : **le logiciel dessine directement dans un bloc de mémoire dont le contenu correspond pixel par pixel à l''écran**. Cette méthode est antérieure aux GPU modernes et reste aujourd''hui à la base du fonctionnement des écrans.

        ## Qu''est-ce qu''un framebuffer

        Un **framebuffer** est une zone réservée de mémoire — prélevée sur la RAM principale du système ou réalisée sous forme de banque de mémoire vidéo dédiée — dans laquelle chaque octet ou groupe d''octets correspond à un pixel ou à un groupe de pixels. Quand le logiciel y écrit des valeurs de couleur, il peint en réalité l''écran.

        Contrairement aux systèmes dotés d''une unité de traitement graphique, une machine à framebuffer n''a **aucune assistance matérielle** pour :

        - le traitement de la géométrie ou des polygones
        - la gestion des sprites
        - le blitting ou la mise à l''échelle
        - le mélange de couleurs ou l''ombrage

        Toutes ces tâches reposent intégralement sur le **processeur principal**, qui doit calculer les valeurs finales des pixels et les écrire directement dans le framebuffer.

        ## Le rôle du RAMDAC

        Pour transformer le contenu numérique du framebuffer en une image visible, la mémoire est reliée à une puce communément appelée **RAMDAC** (Random Access Memory Digital-to-Analog Converter, convertisseur numérique-analogique de mémoire à accès aléatoire). Le RAMDAC :

        - lit les données de pixels depuis le framebuffer
        - convertit ces valeurs en signaux analogiques (ou, plus tard, numériques)
        - envoie le signal obtenu à l''écran

        Crucialement, le RAMDAC n''effectue **aucun traitement graphique**. Il ne dessine pas de formes, ne déplace pas de sprites et n''accélère pas le rendu. Il se contente de traduire les données de pixels brutes du framebuffer dans le format électrique requis par le moniteur.

        ## Forces et faiblesses

        ### Avantages

        - **Souplesse illimitée :** Toute image que le CPU peut calculer peut être affichée — pas de limites matérielles de sprites, pas de tile maps imposées, pas de modes de couleurs figés.
        - **Comportement prévisible :** Le framebuffer est un modèle simple et transparent que les développeurs peuvent manipuler directement.
        - **Universalité :** Beaucoup de systèmes d''exploitation et d''environnements graphiques sont bâtis autour du concept de framebuffer.

        ### Limites

        - **Forte charge CPU :** Tout le travail de rendu entre en concurrence avec la logique de l''application ou du jeu principal.
        - **Goulots d''étranglement :** Lorsque les résolutions et la profondeur de couleur augmentent, le CPU peut peiner à mettre à jour le framebuffer assez vite.
        - **Pas d''accélération matérielle :** Des effets comme le défilement, la mise à l''échelle ou l''animation doivent être implémentés entièrement en logiciel.

        ## Usage historique

        Le graphisme à base de framebuffer apparaît tout au long de l''histoire de l''informatique, des premières stations de travail aux systèmes grand public. Fait notable, cette approche n''était pas réservée aux machines bon marché. Les concepteurs renonçaient parfois à un GPU dédié simplement parce que :

        - on jugeait le processeur principal suffisamment puissant
        - l''usage prévu du système ne nécessitait pas de graphismes évolués
        - la simplicité d''une architecture à framebuffer était souhaitable

        En pratique, beaucoup de ces systèmes ont fini par révéler les limites d''un rendu reposant uniquement sur le CPU, surtout à mesure que les attentes graphiques augmentaient.

        ## Résumé technique

        - **Définition :** Une région de mémoire dont le contenu représente directement les pixels à l''écran.
        - **Modèle de rendu :** Le CPU calcule et écrit l''ensemble des données de pixels.
        - **Matériel de sortie :** Le RAMDAC convertit le contenu du framebuffer en signaux d''affichage.
        - **Charge CPU :** Importante ; augmente avec la résolution et la profondeur de couleur.
        - **Capacité graphique :** Limitée uniquement par la puissance du CPU et la bande passante mémoire.
        - **Rôle historique :** Utilisée dans des systèmes allant des premiers micro-ordinateurs aux stations de travail haut de gamme.
        - **Remplacée par :** Les GPU et les processeurs graphiques intégrés à accélération matérielle.
        ', '<h1>Graphisme à base de framebuffer</h1>
        <h3>Quand le CPU dessine chaque pixel</h3>
        <p>Un ordinateur qui « utilise un framebuffer » s''appuie sur l''une des approches les plus fondamentales du graphisme numérique : <strong>le logiciel dessine directement dans un bloc de mémoire dont le contenu correspond pixel par pixel à l''écran</strong>. Cette méthode est antérieure aux GPU modernes et reste aujourd''hui à la base du fonctionnement des écrans.</p>
        <h2>Qu''est-ce qu''un framebuffer</h2>
        <p>Un <strong>framebuffer</strong> est une zone réservée de mémoire — prélevée sur la RAM principale du système ou réalisée sous forme de banque de mémoire vidéo dédiée — dans laquelle chaque octet ou groupe d''octets correspond à un pixel ou à un groupe de pixels. Quand le logiciel y écrit des valeurs de couleur, il peint en réalité l''écran.</p>
        <p>Contrairement aux systèmes dotés d''une unité de traitement graphique, une machine à framebuffer n''a <strong>aucune assistance matérielle</strong> pour :</p>
        <ul><li>le traitement de la géométrie ou des polygones</li><li>la gestion des sprites</li><li>le blitting ou la mise à l''échelle</li><li>le mélange de couleurs ou l''ombrage</li></ul>
        <p>Toutes ces tâches reposent intégralement sur le <strong>processeur principal</strong>, qui doit calculer les valeurs finales des pixels et les écrire directement dans le framebuffer.</p>
        <h2>Le rôle du RAMDAC</h2>
        <p>Pour transformer le contenu numérique du framebuffer en une image visible, la mémoire est reliée à une puce communément appelée <strong>RAMDAC</strong> (Random Access Memory Digital-to-Analog Converter, convertisseur numérique-analogique de mémoire à accès aléatoire). Le RAMDAC :</p>
        <ul><li>lit les données de pixels depuis le framebuffer</li><li>convertit ces valeurs en signaux analogiques (ou, plus tard, numériques)</li><li>envoie le signal obtenu à l''écran</li></ul>
        <p>Crucialement, le RAMDAC n''effectue <strong>aucun traitement graphique</strong>. Il ne dessine pas de formes, ne déplace pas de sprites et n''accélère pas le rendu. Il se contente de traduire les données de pixels brutes du framebuffer dans le format électrique requis par le moniteur.</p>
        <h2>Forces et faiblesses</h2>
        <h3>Avantages</h3>
        <ul><li><strong>Souplesse illimitée :</strong> Toute image que le CPU peut calculer peut être affichée — pas de limites matérielles de sprites, pas de tile maps imposées, pas de modes de couleurs figés.</li><li><strong>Comportement prévisible :</strong> Le framebuffer est un modèle simple et transparent que les développeurs peuvent manipuler directement.</li><li><strong>Universalité :</strong> Beaucoup de systèmes d''exploitation et d''environnements graphiques sont bâtis autour du concept de framebuffer.</li></ul>
        <h3>Limites</h3>
        <ul><li><strong>Forte charge CPU :</strong> Tout le travail de rendu entre en concurrence avec la logique de l''application ou du jeu principal.</li><li><strong>Goulots d''étranglement :</strong> Lorsque les résolutions et la profondeur de couleur augmentent, le CPU peut peiner à mettre à jour le framebuffer assez vite.</li><li><strong>Pas d''accélération matérielle :</strong> Des effets comme le défilement, la mise à l''échelle ou l''animation doivent être implémentés entièrement en logiciel.</li></ul>
        <h2>Usage historique</h2>
        <p>Le graphisme à base de framebuffer apparaît tout au long de l''histoire de l''informatique, des premières stations de travail aux systèmes grand public. Fait notable, cette approche n''était pas réservée aux machines bon marché. Les concepteurs renonçaient parfois à un GPU dédié simplement parce que :</p>
        <ul><li>on jugeait le processeur principal suffisamment puissant</li><li>l''usage prévu du système ne nécessitait pas de graphismes évolués</li><li>la simplicité d''une architecture à framebuffer était souhaitable</li></ul>
        <p>En pratique, beaucoup de ces systèmes ont fini par révéler les limites d''un rendu reposant uniquement sur le CPU, surtout à mesure que les attentes graphiques augmentaient.</p>
        <h2>Résumé technique</h2>
        <ul><li><strong>Définition :</strong> Une région de mémoire dont le contenu représente directement les pixels à l''écran.</li><li><strong>Modèle de rendu :</strong> Le CPU calcule et écrit l''ensemble des données de pixels.</li><li><strong>Matériel de sortie :</strong> Le RAMDAC convertit le contenu du framebuffer en signaux d''affichage.</li><li><strong>Charge CPU :</strong> Importante ; augmente avec la résolution et la profondeur de couleur.</li><li><strong>Capacité graphique :</strong> Limitée uniquement par la puissance du CPU et la bande passante mémoire.</li><li><strong>Rôle historique :</strong> Utilisée dans des systèmes allant des premiers micro-ordinateurs aux stations de travail haut de gamme.</li><li><strong>Remplacée par :</strong> Les GPU et les processeurs graphiques intégrés à accélération matérielle.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -2 AND `LanguageCode` = 'fra'
            );
        ");

        // -------- ita --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'ita', '# Grafica basata su framebuffer
        ### Quando la CPU disegna ogni pixel

        Un computer che ""usa un framebuffer"" si affida a uno degli approcci più fondamentali della grafica digitale: **il software disegna direttamente in un blocco di memoria il cui contenuto corrisponde uno a uno ai pixel sullo schermo**. Questo metodo precede le moderne GPU e rimane alla base del funzionamento degli schermi ancora oggi.

        ## Che cos''è un framebuffer

        Un **framebuffer** è una regione riservata di memoria — ricavata dalla RAM principale del sistema o realizzata come banco di memoria video dedicato — in cui ogni byte o gruppo di byte corrisponde a un pixel o a un gruppo di pixel. Quando il software vi scrive valori di colore, sta in realtà dipingendo lo schermo.

        A differenza dei sistemi con un''unità di elaborazione grafica, una macchina a framebuffer non ha **alcun supporto hardware** per:

        - elaborazione di geometria o poligoni
        - gestione degli sprite
        - blitting o scalatura
        - miscelazione di colori o shading

        Tutti questi compiti ricadono interamente sul **processore principale**, che deve calcolare i valori finali dei pixel e scriverli direttamente nel framebuffer.

        ## Il ruolo del RAMDAC

        Per trasformare il contenuto digitale del framebuffer in un''immagine visibile, la memoria è collegata a un chip comunemente noto come **RAMDAC** (Random Access Memory Digital-to-Analog Converter, convertitore digitale-analogico di memoria ad accesso casuale). Il RAMDAC:

        - legge i dati dei pixel dal framebuffer
        - converte tali valori in segnali analogici (o, in seguito, digitali)
        - invia il segnale risultante al display

        Fondamentalmente, il RAMDAC non esegue **alcuna elaborazione grafica**. Non disegna forme, non muove sprite e non accelera il rendering. Si limita a tradurre i dati grezzi dei pixel del framebuffer nel formato elettrico richiesto dal monitor.

        ## Punti di forza e di debolezza

        ### Vantaggi

        - **Flessibilità illimitata:** Si può mostrare qualsiasi immagine che la CPU sia in grado di calcolare — niente limiti di sprite imposti dall''hardware, niente tile map fisse, niente modalità di colore predeterminate.
        - **Comportamento prevedibile:** Il framebuffer è un modello semplice e trasparente che gli sviluppatori possono manipolare direttamente.
        - **Universalità:** Molti sistemi operativi e ambienti grafici sono costruiti attorno al concetto di framebuffer.

        ### Limiti

        - **Carico CPU elevato:** Tutto il lavoro di rendering compete con la logica dell''applicazione o del gioco principale.
        - **Colli di bottiglia prestazionali:** All''aumentare di risoluzioni e profondità di colore, la CPU può fare fatica ad aggiornare il framebuffer abbastanza in fretta.
        - **Nessuna accelerazione hardware:** Effetti come scorrimento, scalatura o animazione devono essere realizzati interamente via software.

        ## Uso storico

        La grafica basata su framebuffer compare lungo tutta la storia dell''informatica, dalle prime workstation ai sistemi consumer. È importante notare che questo approccio non era limitato alle macchine economiche. A volte i progettisti omettevano una GPU dedicata semplicemente perché:

        - si riteneva che il processore principale fosse abbastanza potente
        - l''uso previsto del sistema non richiedeva grafica avanzata
        - era desiderabile la semplicità di un''architettura a framebuffer

        In pratica, molti di questi sistemi finirono per rivelare i limiti di un rendering affidato unicamente alla CPU, soprattutto man mano che cresceva l''aspettativa grafica.

        ## Riepilogo tecnico

        - **Definizione:** Una regione di memoria il cui contenuto rappresenta direttamente i pixel sullo schermo.
        - **Modello di rendering:** La CPU calcola e scrive tutti i dati dei pixel.
        - **Hardware di uscita:** Il RAMDAC converte il contenuto del framebuffer in segnali per il display.
        - **Carico CPU:** Significativo; aumenta con la risoluzione e la profondità di colore.
        - **Capacità grafica:** Limitata solo dalla potenza della CPU e dalla larghezza di banda della memoria.
        - **Ruolo storico:** Usato in sistemi che vanno dai primi microcomputer alle workstation di alta gamma.
        - **Sostituita da:** GPU e processori grafici integrati con accelerazione hardware.
        ', '<h1>Grafica basata su framebuffer</h1>
        <h3>Quando la CPU disegna ogni pixel</h3>
        <p>Un computer che ""usa un framebuffer"" si affida a uno degli approcci più fondamentali della grafica digitale: <strong>il software disegna direttamente in un blocco di memoria il cui contenuto corrisponde uno a uno ai pixel sullo schermo</strong>. Questo metodo precede le moderne GPU e rimane alla base del funzionamento degli schermi ancora oggi.</p>
        <h2>Che cos''è un framebuffer</h2>
        <p>Un <strong>framebuffer</strong> è una regione riservata di memoria — ricavata dalla RAM principale del sistema o realizzata come banco di memoria video dedicato — in cui ogni byte o gruppo di byte corrisponde a un pixel o a un gruppo di pixel. Quando il software vi scrive valori di colore, sta in realtà dipingendo lo schermo.</p>
        <p>A differenza dei sistemi con un''unità di elaborazione grafica, una macchina a framebuffer non ha <strong>alcun supporto hardware</strong> per:</p>
        <ul><li>elaborazione di geometria o poligoni</li><li>gestione degli sprite</li><li>blitting o scalatura</li><li>miscelazione di colori o shading</li></ul>
        <p>Tutti questi compiti ricadono interamente sul <strong>processore principale</strong>, che deve calcolare i valori finali dei pixel e scriverli direttamente nel framebuffer.</p>
        <h2>Il ruolo del RAMDAC</h2>
        <p>Per trasformare il contenuto digitale del framebuffer in un''immagine visibile, la memoria è collegata a un chip comunemente noto come <strong>RAMDAC</strong> (Random Access Memory Digital-to-Analog Converter, convertitore digitale-analogico di memoria ad accesso casuale). Il RAMDAC:</p>
        <ul><li>legge i dati dei pixel dal framebuffer</li><li>converte tali valori in segnali analogici (o, in seguito, digitali)</li><li>invia il segnale risultante al display</li></ul>
        <p>Fondamentalmente, il RAMDAC non esegue <strong>alcuna elaborazione grafica</strong>. Non disegna forme, non muove sprite e non accelera il rendering. Si limita a tradurre i dati grezzi dei pixel del framebuffer nel formato elettrico richiesto dal monitor.</p>
        <h2>Punti di forza e di debolezza</h2>
        <h3>Vantaggi</h3>
        <ul><li><strong>Flessibilità illimitata:</strong> Si può mostrare qualsiasi immagine che la CPU sia in grado di calcolare — niente limiti di sprite imposti dall''hardware, niente tile map fisse, niente modalità di colore predeterminate.</li><li><strong>Comportamento prevedibile:</strong> Il framebuffer è un modello semplice e trasparente che gli sviluppatori possono manipolare direttamente.</li><li><strong>Universalità:</strong> Molti sistemi operativi e ambienti grafici sono costruiti attorno al concetto di framebuffer.</li></ul>
        <h3>Limiti</h3>
        <ul><li><strong>Carico CPU elevato:</strong> Tutto il lavoro di rendering compete con la logica dell''applicazione o del gioco principale.</li><li><strong>Colli di bottiglia prestazionali:</strong> All''aumentare di risoluzioni e profondità di colore, la CPU può fare fatica ad aggiornare il framebuffer abbastanza in fretta.</li><li><strong>Nessuna accelerazione hardware:</strong> Effetti come scorrimento, scalatura o animazione devono essere realizzati interamente via software.</li></ul>
        <h2>Uso storico</h2>
        <p>La grafica basata su framebuffer compare lungo tutta la storia dell''informatica, dalle prime workstation ai sistemi consumer. È importante notare che questo approccio non era limitato alle macchine economiche. A volte i progettisti omettevano una GPU dedicata semplicemente perché:</p>
        <ul><li>si riteneva che il processore principale fosse abbastanza potente</li><li>l''uso previsto del sistema non richiedeva grafica avanzata</li><li>era desiderabile la semplicità di un''architettura a framebuffer</li></ul>
        <p>In pratica, molti di questi sistemi finirono per rivelare i limiti di un rendering affidato unicamente alla CPU, soprattutto man mano che cresceva l''aspettativa grafica.</p>
        <h2>Riepilogo tecnico</h2>
        <ul><li><strong>Definizione:</strong> Una regione di memoria il cui contenuto rappresenta direttamente i pixel sullo schermo.</li><li><strong>Modello di rendering:</strong> La CPU calcola e scrive tutti i dati dei pixel.</li><li><strong>Hardware di uscita:</strong> Il RAMDAC converte il contenuto del framebuffer in segnali per il display.</li><li><strong>Carico CPU:</strong> Significativo; aumenta con la risoluzione e la profondità di colore.</li><li><strong>Capacità grafica:</strong> Limitata solo dalla potenza della CPU e dalla larghezza di banda della memoria.</li><li><strong>Ruolo storico:</strong> Usato in sistemi che vanno dai primi microcomputer alle workstation di alta gamma.</li><li><strong>Sostituita da:</strong> GPU e processori grafici integrati con accelerazione hardware.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -2 AND `LanguageCode` = 'ita'
            );
        ");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM `GpuDescriptions`
            WHERE `GpuId` = -2
            AND `LanguageCode` IN ('eng', 'spa', 'deu', 'fra', 'ita');
        ");
    }
}
