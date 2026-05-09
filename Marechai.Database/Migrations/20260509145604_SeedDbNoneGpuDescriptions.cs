using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations;

/// <summary>
/// Seeds the description for the DB_NONE sentinel GPU
/// (id = -1, see <c>Marechai.Database.Operations.DbNone</c>) in all
/// five languages supported by the application UI: English (eng), Spanish
/// (spa), German (deu), French (fra) and Italian (ita).
///
/// The DB_NONE row itself is seeded by the legacy pre-EF migration
/// <c>UpdateDatabaseToV12</c> in <c>Marechai.Database/Operations/Update.cs</c>.
/// This migration only fills the `GpuDescriptions` table for it.
///
/// Inserts are idempotent (gated by `WHERE NOT EXISTS`) so re-running the
/// migration on a DB where some or all rows already exist will not fail
/// with a duplicate-key error.
/// </summary>
public partial class SeedDbNoneGpuDescriptions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // -------- eng --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -1, 'eng', '# Software-Driven Display Control
        ### When a Computer Has No GPU but Still Draws a Screen

        In early and ultra-low-cost computing, some machines produced video output **without any dedicated graphics processor**. Instead, the **main CPU** was responsible for generating every pixel on the display. This technique—known as **software-driven display control**—represents one of the most minimalistic approaches to computer graphics ever used.

        ## Historical Background

        Before affordable video controllers and integrated graphics chips became widespread, many early microcomputers and embedded devices relied on the CPU to toggle electrical lines connected directly to a display. This approach was especially common in:

        - **Calculators and scientific instruments**, where displays were small and static.
        - **Early microcomputers with simple LCDs**, often assembled as hobbyist or do-it-yourself kits.
        - **Cost-reduced systems**, where eliminating a graphics chip significantly lowered manufacturing expenses.

        As display resolutions increased and graphical interfaces became more sophisticated, this method quickly disappeared. By the mid-1980s, dedicated video hardware had become inexpensive enough that software-driven displays were largely obsolete.

        ## How Software-Driven Display Control Works

        In a system without a GPU, the CPU must handle **every aspect of drawing**:

        - **Pixel control:** The processor directly manipulates the electrical pins that correspond to rows and columns of the display.
        - **Timing:** The CPU must maintain precise refresh cycles to keep the image stable.
        - **Rendering:** All graphics—text, shapes, animations—are computed and output in real time by software routines.

        This method is feasible only when the display has **very few pixels**. A small monochrome LCD, for example, might require only a few dozen control lines, making it possible for the CPU to manage them directly.

        ## Advantages and Limitations

        ### Advantages

        - **Extremely low cost:** No graphics chip, no video memory, minimal circuitry.
        - **Simplicity:** Ideal for early hobbyist systems and educational kits.
        - **Full software control:** Developers could manipulate the display at the most fundamental electrical level.

        ### Limitations

        - **Highly inefficient:** The CPU spends a large portion of its time refreshing the display instead of running applications.
        - **Poor scalability:** As soon as displays grew larger or more complex, this approach became impractical.
        - **Limited graphics capability:** Suitable only for simple text or low-resolution pixel patterns.

        ## Decline and Legacy

        Software-driven display control vanished rapidly as soon as dedicated video controllers became affordable. These chips could buffer images, generate timing signals, and handle refresh cycles automatically—freeing the CPU and enabling richer graphics.

        Despite its short lifespan, this technique played a crucial role in the early evolution of computing. It allowed the first wave of inexpensive microcomputers and embedded devices to exist at all, demonstrating how much could be achieved with minimal hardware and clever software.

        ## Technical Summary

        - **Definition:** Display output generated entirely by CPU-controlled electrical signals.
        - **Hardware Requirements:** Simple LCD or segment display; no GPU or video controller.
        - **CPU Load:** Very high; continuous refresh required.
        - **Typical Use:** Calculators, early microcomputers, DIY kits, cost-reduced systems.
        - **Graphics Capability:** Minimal; suitable only for low-resolution or segmented displays.
        - **Superseded By:** Dedicated video controllers and integrated GPUs.
        ', '<h1>Software-Driven Display Control</h1>
        <h3>When a Computer Has No GPU but Still Draws a Screen</h3>
        <p>In early and ultra-low-cost computing, some machines produced video output <strong>without any dedicated graphics processor</strong>. Instead, the <strong>main CPU</strong> was responsible for generating every pixel on the display. This technique—known as <strong>software-driven display control</strong>—represents one of the most minimalistic approaches to computer graphics ever used.</p>
        <h2>Historical Background</h2>
        <p>Before affordable video controllers and integrated graphics chips became widespread, many early microcomputers and embedded devices relied on the CPU to toggle electrical lines connected directly to a display. This approach was especially common in:</p>
        <ul><li><strong>Calculators and scientific instruments</strong>, where displays were small and static.</li><li><strong>Early microcomputers with simple LCDs</strong>, often assembled as hobbyist or do-it-yourself kits.</li><li><strong>Cost-reduced systems</strong>, where eliminating a graphics chip significantly lowered manufacturing expenses.</li></ul>
        <p>As display resolutions increased and graphical interfaces became more sophisticated, this method quickly disappeared. By the mid-1980s, dedicated video hardware had become inexpensive enough that software-driven displays were largely obsolete.</p>
        <h2>How Software-Driven Display Control Works</h2>
        <p>In a system without a GPU, the CPU must handle <strong>every aspect of drawing</strong>:</p>
        <ul><li><strong>Pixel control:</strong> The processor directly manipulates the electrical pins that correspond to rows and columns of the display.</li><li><strong>Timing:</strong> The CPU must maintain precise refresh cycles to keep the image stable.</li><li><strong>Rendering:</strong> All graphics—text, shapes, animations—are computed and output in real time by software routines.</li></ul>
        <p>This method is feasible only when the display has <strong>very few pixels</strong>. A small monochrome LCD, for example, might require only a few dozen control lines, making it possible for the CPU to manage them directly.</p>
        <h2>Advantages and Limitations</h2>
        <h3>Advantages</h3>
        <ul><li><strong>Extremely low cost:</strong> No graphics chip, no video memory, minimal circuitry.</li><li><strong>Simplicity:</strong> Ideal for early hobbyist systems and educational kits.</li><li><strong>Full software control:</strong> Developers could manipulate the display at the most fundamental electrical level.</li></ul>
        <h3>Limitations</h3>
        <ul><li><strong>Highly inefficient:</strong> The CPU spends a large portion of its time refreshing the display instead of running applications.</li><li><strong>Poor scalability:</strong> As soon as displays grew larger or more complex, this approach became impractical.</li><li><strong>Limited graphics capability:</strong> Suitable only for simple text or low-resolution pixel patterns.</li></ul>
        <h2>Decline and Legacy</h2>
        <p>Software-driven display control vanished rapidly as soon as dedicated video controllers became affordable. These chips could buffer images, generate timing signals, and handle refresh cycles automatically—freeing the CPU and enabling richer graphics.</p>
        <p>Despite its short lifespan, this technique played a crucial role in the early evolution of computing. It allowed the first wave of inexpensive microcomputers and embedded devices to exist at all, demonstrating how much could be achieved with minimal hardware and clever software.</p>
        <h2>Technical Summary</h2>
        <ul><li><strong>Definition:</strong> Display output generated entirely by CPU-controlled electrical signals.</li><li><strong>Hardware Requirements:</strong> Simple LCD or segment display; no GPU or video controller.</li><li><strong>CPU Load:</strong> Very high; continuous refresh required.</li><li><strong>Typical Use:</strong> Calculators, early microcomputers, DIY kits, cost-reduced systems.</li><li><strong>Graphics Capability:</strong> Minimal; suitable only for low-resolution or segmented displays.</li><li><strong>Superseded By:</strong> Dedicated video controllers and integrated GPUs.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -1 AND `LanguageCode` = 'eng'
            );
        ");

        // -------- spa --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -1, 'spa', '# Control de pantalla por software
        ### Cuando un ordenador no tiene GPU pero aun así dibuja en pantalla

        En los primeros tiempos de la informática y en los sistemas de muy bajo coste, algunas máquinas producían salida de vídeo **sin ningún procesador gráfico dedicado**. En su lugar, el **procesador principal** se encargaba de generar cada píxel de la pantalla. Esta técnica —conocida como **control de pantalla por software**— representa uno de los enfoques más minimalistas jamás empleados en gráficos por ordenador.

        ## Contexto histórico

        Antes de que los controladores de vídeo asequibles y los chips gráficos integrados se generalizaran, muchos primeros microordenadores y dispositivos embebidos dependían de la CPU para conmutar líneas eléctricas conectadas directamente a una pantalla. Este enfoque era especialmente común en:

        - **Calculadoras e instrumentos científicos**, donde las pantallas eran pequeñas y estáticas.
        - **Primeros microordenadores con LCD sencillos**, a menudo montados como kits para aficionados o de bricolaje.
        - **Sistemas de coste reducido**, donde eliminar un chip gráfico abarataba notablemente la fabricación.

        A medida que las resoluciones de pantalla aumentaron y las interfaces gráficas se hicieron más sofisticadas, este método desapareció rápidamente. A mediados de los años ochenta, el hardware de vídeo dedicado se había vuelto lo bastante barato como para que las pantallas controladas por software quedaran prácticamente obsoletas.

        ## Cómo funciona el control de pantalla por software

        En un sistema sin GPU, la CPU debe encargarse de **todos los aspectos del dibujo**:

        - **Control de píxeles:** El procesador manipula directamente los pines eléctricos correspondientes a las filas y columnas de la pantalla.
        - **Temporización:** La CPU debe mantener ciclos de refresco precisos para que la imagen permanezca estable.
        - **Renderizado:** Todos los gráficos —texto, formas, animaciones— se calculan y emiten en tiempo real mediante rutinas de software.

        Este método solo es viable cuando la pantalla tiene **muy pocos píxeles**. Un pequeño LCD monocromo, por ejemplo, puede requerir apenas unas docenas de líneas de control, lo que permite que la CPU las gestione directamente.

        ## Ventajas y limitaciones

        ### Ventajas

        - **Coste extremadamente bajo:** Sin chip gráfico, sin memoria de vídeo, con circuitería mínima.
        - **Sencillez:** Ideal para los primeros sistemas para aficionados y los kits educativos.
        - **Control total por software:** Los desarrolladores podían manipular la pantalla al nivel eléctrico más básico.

        ### Limitaciones

        - **Muy ineficiente:** La CPU dedica una parte considerable de su tiempo a refrescar la pantalla en lugar de ejecutar aplicaciones.
        - **Mala escalabilidad:** En cuanto las pantallas crecían o se volvían más complejas, este enfoque dejaba de ser práctico.
        - **Capacidad gráfica limitada:** Solo apto para texto sencillo o patrones de píxeles de baja resolución.

        ## Declive y legado

        El control de pantalla por software desapareció rápidamente en cuanto los controladores de vídeo dedicados se volvieron asequibles. Estos chips podían almacenar imágenes en búfer, generar señales de temporización y gestionar los ciclos de refresco de forma automática, liberando a la CPU y permitiendo gráficos más ricos.

        Pese a su corta vida, esta técnica desempeñó un papel crucial en la evolución temprana de la informática. Permitió la propia existencia de la primera oleada de microordenadores y dispositivos embebidos asequibles, demostrando cuánto se podía lograr con hardware mínimo y software ingenioso.

        ## Resumen técnico

        - **Definición:** Salida de pantalla generada íntegramente mediante señales eléctricas controladas por la CPU.
        - **Requisitos de hardware:** LCD sencillo o pantalla por segmentos; sin GPU ni controlador de vídeo.
        - **Carga de CPU:** Muy alta; refresco continuo necesario.
        - **Uso típico:** Calculadoras, primeros microordenadores, kits de bricolaje, sistemas de coste reducido.
        - **Capacidad gráfica:** Mínima; solo apta para pantallas de baja resolución o segmentadas.
        - **Sustituido por:** Controladores de vídeo dedicados y GPU integradas.
        ', '<h1>Control de pantalla por software</h1>
        <h3>Cuando un ordenador no tiene GPU pero aun así dibuja en pantalla</h3>
        <p>En los primeros tiempos de la informática y en los sistemas de muy bajo coste, algunas máquinas producían salida de vídeo <strong>sin ningún procesador gráfico dedicado</strong>. En su lugar, el <strong>procesador principal</strong> se encargaba de generar cada píxel de la pantalla. Esta técnica —conocida como <strong>control de pantalla por software</strong>— representa uno de los enfoques más minimalistas jamás empleados en gráficos por ordenador.</p>
        <h2>Contexto histórico</h2>
        <p>Antes de que los controladores de vídeo asequibles y los chips gráficos integrados se generalizaran, muchos primeros microordenadores y dispositivos embebidos dependían de la CPU para conmutar líneas eléctricas conectadas directamente a una pantalla. Este enfoque era especialmente común en:</p>
        <ul><li><strong>Calculadoras e instrumentos científicos</strong>, donde las pantallas eran pequeñas y estáticas.</li><li><strong>Primeros microordenadores con LCD sencillos</strong>, a menudo montados como kits para aficionados o de bricolaje.</li><li><strong>Sistemas de coste reducido</strong>, donde eliminar un chip gráfico abarataba notablemente la fabricación.</li></ul>
        <p>A medida que las resoluciones de pantalla aumentaron y las interfaces gráficas se hicieron más sofisticadas, este método desapareció rápidamente. A mediados de los años ochenta, el hardware de vídeo dedicado se había vuelto lo bastante barato como para que las pantallas controladas por software quedaran prácticamente obsoletas.</p>
        <h2>Cómo funciona el control de pantalla por software</h2>
        <p>En un sistema sin GPU, la CPU debe encargarse de <strong>todos los aspectos del dibujo</strong>:</p>
        <ul><li><strong>Control de píxeles:</strong> El procesador manipula directamente los pines eléctricos correspondientes a las filas y columnas de la pantalla.</li><li><strong>Temporización:</strong> La CPU debe mantener ciclos de refresco precisos para que la imagen permanezca estable.</li><li><strong>Renderizado:</strong> Todos los gráficos —texto, formas, animaciones— se calculan y emiten en tiempo real mediante rutinas de software.</li></ul>
        <p>Este método solo es viable cuando la pantalla tiene <strong>muy pocos píxeles</strong>. Un pequeño LCD monocromo, por ejemplo, puede requerir apenas unas docenas de líneas de control, lo que permite que la CPU las gestione directamente.</p>
        <h2>Ventajas y limitaciones</h2>
        <h3>Ventajas</h3>
        <ul><li><strong>Coste extremadamente bajo:</strong> Sin chip gráfico, sin memoria de vídeo, con circuitería mínima.</li><li><strong>Sencillez:</strong> Ideal para los primeros sistemas para aficionados y los kits educativos.</li><li><strong>Control total por software:</strong> Los desarrolladores podían manipular la pantalla al nivel eléctrico más básico.</li></ul>
        <h3>Limitaciones</h3>
        <ul><li><strong>Muy ineficiente:</strong> La CPU dedica una parte considerable de su tiempo a refrescar la pantalla en lugar de ejecutar aplicaciones.</li><li><strong>Mala escalabilidad:</strong> En cuanto las pantallas crecían o se volvían más complejas, este enfoque dejaba de ser práctico.</li><li><strong>Capacidad gráfica limitada:</strong> Solo apto para texto sencillo o patrones de píxeles de baja resolución.</li></ul>
        <h2>Declive y legado</h2>
        <p>El control de pantalla por software desapareció rápidamente en cuanto los controladores de vídeo dedicados se volvieron asequibles. Estos chips podían almacenar imágenes en búfer, generar señales de temporización y gestionar los ciclos de refresco de forma automática, liberando a la CPU y permitiendo gráficos más ricos.</p>
        <p>Pese a su corta vida, esta técnica desempeñó un papel crucial en la evolución temprana de la informática. Permitió la propia existencia de la primera oleada de microordenadores y dispositivos embebidos asequibles, demostrando cuánto se podía lograr con hardware mínimo y software ingenioso.</p>
        <h2>Resumen técnico</h2>
        <ul><li><strong>Definición:</strong> Salida de pantalla generada íntegramente mediante señales eléctricas controladas por la CPU.</li><li><strong>Requisitos de hardware:</strong> LCD sencillo o pantalla por segmentos; sin GPU ni controlador de vídeo.</li><li><strong>Carga de CPU:</strong> Muy alta; refresco continuo necesario.</li><li><strong>Uso típico:</strong> Calculadoras, primeros microordenadores, kits de bricolaje, sistemas de coste reducido.</li><li><strong>Capacidad gráfica:</strong> Mínima; solo apta para pantallas de baja resolución o segmentadas.</li><li><strong>Sustituido por:</strong> Controladores de vídeo dedicados y GPU integradas.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -1 AND `LanguageCode` = 'spa'
            );
        ");

        // -------- deu --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -1, 'deu', '# Softwaregesteuerte Bildschirmsteuerung
        ### Wenn ein Computer keine GPU besitzt und trotzdem ein Bild ausgibt

        In der Frühzeit des Computings und bei besonders kostengünstigen Geräten erzeugten manche Maschinen eine Bildschirmausgabe **ohne jeglichen dedizierten Grafikprozessor**. Stattdessen war der **Hauptprozessor** dafür verantwortlich, jeden Bildpunkt der Anzeige zu erzeugen. Diese Technik — als **softwaregesteuerte Bildschirmsteuerung** bekannt — gehört zu den minimalistischsten Ansätzen, die je in der Computergrafik verwendet wurden.

        ## Historischer Hintergrund

        Bevor erschwingliche Video-Controller und integrierte Grafikchips weit verbreitet waren, waren viele frühe Mikrocomputer und eingebettete Geräte darauf angewiesen, dass die CPU elektrische Leitungen direkt zur Anzeige umschaltete. Dieser Ansatz war besonders verbreitet bei:

        - **Taschenrechnern und wissenschaftlichen Instrumenten**, deren Anzeigen klein und statisch waren.
        - **Frühen Mikrocomputern mit einfachen LCDs**, häufig als Bastler- oder Selbstbausätze.
        - **Kostenreduzierten Systemen**, in denen das Weglassen eines Grafikchips die Fertigungskosten deutlich senkte.

        Mit steigenden Bildschirmauflösungen und immer ausgefeilteren grafischen Oberflächen verschwand diese Methode rasch. Mitte der 1980er-Jahre war dedizierte Videohardware so erschwinglich geworden, dass softwaregesteuerte Anzeigen praktisch obsolet waren.

        ## Funktionsweise

        In einem System ohne GPU muss die CPU **jeden Aspekt der Bilderzeugung** übernehmen:

        - **Pixelsteuerung:** Der Prozessor steuert direkt die elektrischen Pins, die den Zeilen und Spalten der Anzeige entsprechen.
        - **Zeitsteuerung:** Die CPU muss präzise Bildwiederholzyklen einhalten, damit das Bild stabil bleibt.
        - **Rendering:** Sämtliche Grafiken — Text, Formen, Animationen — werden in Echtzeit von Softwareroutinen berechnet und ausgegeben.

        Diese Methode ist nur dann sinnvoll, wenn die Anzeige **sehr wenige Pixel** hat. Ein kleines monochromes LCD beispielsweise kann mit nur wenigen Dutzend Steuerleitungen auskommen, sodass die CPU sie direkt verwalten kann.

        ## Vorteile und Grenzen

        ### Vorteile

        - **Extrem niedrige Kosten:** Kein Grafikchip, kein Videospeicher, minimale Schaltung.
        - **Einfachheit:** Ideal für frühe Bastlersysteme und Lernbausätze.
        - **Vollständige Softwarekontrolle:** Entwickler konnten die Anzeige auf der grundlegendsten elektrischen Ebene ansprechen.

        ### Grenzen

        - **Sehr ineffizient:** Die CPU verbringt einen großen Teil ihrer Zeit damit, das Display aufzufrischen, statt Anwendungen auszuführen.
        - **Schlechte Skalierbarkeit:** Sobald die Displays größer oder komplexer wurden, war dieser Ansatz nicht mehr praktikabel.
        - **Begrenzte Grafikfähigkeit:** Nur für einfachen Text oder niedrigauflösende Pixelmuster geeignet.

        ## Niedergang und Erbe

        Die softwaregesteuerte Bildschirmsteuerung verschwand schnell, sobald dedizierte Video-Controller erschwinglich wurden. Diese Chips konnten Bilder puffern, Taktsignale erzeugen und Refresh-Zyklen automatisch handhaben — die CPU wurde entlastet und reichhaltigere Grafiken wurden möglich.

        Trotz ihrer kurzen Lebensdauer spielte diese Technik eine entscheidende Rolle in der frühen Entwicklung der Computertechnik. Sie ermöglichte überhaupt erst die erste Welle erschwinglicher Mikrocomputer und eingebetteter Geräte und zeigte, wie viel sich mit minimaler Hardware und cleverer Software erreichen lässt.

        ## Technische Zusammenfassung

        - **Definition:** Bildausgabe vollständig durch CPU-gesteuerte elektrische Signale erzeugt.
        - **Hardwareanforderungen:** Einfaches LCD oder Segmentanzeige; keine GPU oder Video-Controller.
        - **CPU-Last:** Sehr hoch; ständiger Refresh erforderlich.
        - **Typischer Einsatz:** Taschenrechner, frühe Mikrocomputer, DIY-Kits, kostenreduzierte Systeme.
        - **Grafikfähigkeit:** Minimal; nur für niedrigauflösende oder segmentierte Anzeigen geeignet.
        - **Abgelöst durch:** Dedizierte Video-Controller und integrierte GPUs.
        ', '<h1>Softwaregesteuerte Bildschirmsteuerung</h1>
        <h3>Wenn ein Computer keine GPU besitzt und trotzdem ein Bild ausgibt</h3>
        <p>In der Frühzeit des Computings und bei besonders kostengünstigen Geräten erzeugten manche Maschinen eine Bildschirmausgabe <strong>ohne jeglichen dedizierten Grafikprozessor</strong>. Stattdessen war der <strong>Hauptprozessor</strong> dafür verantwortlich, jeden Bildpunkt der Anzeige zu erzeugen. Diese Technik — als <strong>softwaregesteuerte Bildschirmsteuerung</strong> bekannt — gehört zu den minimalistischsten Ansätzen, die je in der Computergrafik verwendet wurden.</p>
        <h2>Historischer Hintergrund</h2>
        <p>Bevor erschwingliche Video-Controller und integrierte Grafikchips weit verbreitet waren, waren viele frühe Mikrocomputer und eingebettete Geräte darauf angewiesen, dass die CPU elektrische Leitungen direkt zur Anzeige umschaltete. Dieser Ansatz war besonders verbreitet bei:</p>
        <ul><li><strong>Taschenrechnern und wissenschaftlichen Instrumenten</strong>, deren Anzeigen klein und statisch waren.</li><li><strong>Frühen Mikrocomputern mit einfachen LCDs</strong>, häufig als Bastler- oder Selbstbausätze.</li><li><strong>Kostenreduzierten Systemen</strong>, in denen das Weglassen eines Grafikchips die Fertigungskosten deutlich senkte.</li></ul>
        <p>Mit steigenden Bildschirmauflösungen und immer ausgefeilteren grafischen Oberflächen verschwand diese Methode rasch. Mitte der 1980er-Jahre war dedizierte Videohardware so erschwinglich geworden, dass softwaregesteuerte Anzeigen praktisch obsolet waren.</p>
        <h2>Funktionsweise</h2>
        <p>In einem System ohne GPU muss die CPU <strong>jeden Aspekt der Bilderzeugung</strong> übernehmen:</p>
        <ul><li><strong>Pixelsteuerung:</strong> Der Prozessor steuert direkt die elektrischen Pins, die den Zeilen und Spalten der Anzeige entsprechen.</li><li><strong>Zeitsteuerung:</strong> Die CPU muss präzise Bildwiederholzyklen einhalten, damit das Bild stabil bleibt.</li><li><strong>Rendering:</strong> Sämtliche Grafiken — Text, Formen, Animationen — werden in Echtzeit von Softwareroutinen berechnet und ausgegeben.</li></ul>
        <p>Diese Methode ist nur dann sinnvoll, wenn die Anzeige <strong>sehr wenige Pixel</strong> hat. Ein kleines monochromes LCD beispielsweise kann mit nur wenigen Dutzend Steuerleitungen auskommen, sodass die CPU sie direkt verwalten kann.</p>
        <h2>Vorteile und Grenzen</h2>
        <h3>Vorteile</h3>
        <ul><li><strong>Extrem niedrige Kosten:</strong> Kein Grafikchip, kein Videospeicher, minimale Schaltung.</li><li><strong>Einfachheit:</strong> Ideal für frühe Bastlersysteme und Lernbausätze.</li><li><strong>Vollständige Softwarekontrolle:</strong> Entwickler konnten die Anzeige auf der grundlegendsten elektrischen Ebene ansprechen.</li></ul>
        <h3>Grenzen</h3>
        <ul><li><strong>Sehr ineffizient:</strong> Die CPU verbringt einen großen Teil ihrer Zeit damit, das Display aufzufrischen, statt Anwendungen auszuführen.</li><li><strong>Schlechte Skalierbarkeit:</strong> Sobald die Displays größer oder komplexer wurden, war dieser Ansatz nicht mehr praktikabel.</li><li><strong>Begrenzte Grafikfähigkeit:</strong> Nur für einfachen Text oder niedrigauflösende Pixelmuster geeignet.</li></ul>
        <h2>Niedergang und Erbe</h2>
        <p>Die softwaregesteuerte Bildschirmsteuerung verschwand schnell, sobald dedizierte Video-Controller erschwinglich wurden. Diese Chips konnten Bilder puffern, Taktsignale erzeugen und Refresh-Zyklen automatisch handhaben — die CPU wurde entlastet und reichhaltigere Grafiken wurden möglich.</p>
        <p>Trotz ihrer kurzen Lebensdauer spielte diese Technik eine entscheidende Rolle in der frühen Entwicklung der Computertechnik. Sie ermöglichte überhaupt erst die erste Welle erschwinglicher Mikrocomputer und eingebetteter Geräte und zeigte, wie viel sich mit minimaler Hardware und cleverer Software erreichen lässt.</p>
        <h2>Technische Zusammenfassung</h2>
        <ul><li><strong>Definition:</strong> Bildausgabe vollständig durch CPU-gesteuerte elektrische Signale erzeugt.</li><li><strong>Hardwareanforderungen:</strong> Einfaches LCD oder Segmentanzeige; keine GPU oder Video-Controller.</li><li><strong>CPU-Last:</strong> Sehr hoch; ständiger Refresh erforderlich.</li><li><strong>Typischer Einsatz:</strong> Taschenrechner, frühe Mikrocomputer, DIY-Kits, kostenreduzierte Systeme.</li><li><strong>Grafikfähigkeit:</strong> Minimal; nur für niedrigauflösende oder segmentierte Anzeigen geeignet.</li><li><strong>Abgelöst durch:</strong> Dedizierte Video-Controller und integrierte GPUs.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -1 AND `LanguageCode` = 'deu'
            );
        ");

        // -------- fra --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -1, 'fra', '# Contrôle d''affichage par logiciel
        ### Quand un ordinateur n''a pas de GPU mais affiche tout de même une image

        Aux premiers temps de l''informatique et dans les systèmes à très bas coût, certaines machines produisaient une sortie vidéo **sans aucun processeur graphique dédié**. À la place, le **processeur principal** était chargé de générer chaque pixel à l''écran. Cette technique — appelée **contrôle d''affichage par logiciel** — représente l''une des approches les plus minimalistes jamais utilisées en infographie.

        ## Contexte historique

        Avant que les contrôleurs vidéo abordables et les puces graphiques intégrées ne se généralisent, beaucoup de premiers micro-ordinateurs et dispositifs embarqués s''appuyaient sur la CPU pour commuter des lignes électriques directement reliées à un afficheur. Cette approche était particulièrement courante dans :

        - **Les calculatrices et instruments scientifiques**, dont les écrans étaient petits et statiques.
        - **Les premiers micro-ordinateurs à LCD simples**, souvent assemblés sous forme de kits pour amateurs ou de bricolage.
        - **Les systèmes à coût réduit**, où la suppression d''une puce graphique abaissait significativement les coûts de fabrication.

        À mesure que les résolutions augmentaient et que les interfaces graphiques se sophistiquaient, cette méthode a rapidement disparu. Au milieu des années 1980, le matériel vidéo dédié était devenu suffisamment abordable pour rendre les affichages pilotés par logiciel pratiquement obsolètes.

        ## Fonctionnement

        Dans un système sans GPU, le CPU doit gérer **tous les aspects du dessin** :

        - **Contrôle des pixels :** Le processeur manipule directement les broches électriques correspondant aux lignes et colonnes de l''afficheur.
        - **Cadencement :** Le CPU doit maintenir des cycles de rafraîchissement précis pour que l''image reste stable.
        - **Rendu :** Tous les graphismes — texte, formes, animations — sont calculés et émis en temps réel par des routines logicielles.

        Cette méthode n''est viable que lorsque l''écran comporte **très peu de pixels**. Un petit LCD monochrome, par exemple, peut ne nécessiter que quelques dizaines de lignes de contrôle, ce qui permet à la CPU de les gérer directement.

        ## Avantages et limites

        ### Avantages

        - **Coût extrêmement bas :** Pas de puce graphique, pas de mémoire vidéo, circuiterie minimale.
        - **Simplicité :** Idéale pour les premiers systèmes amateurs et les kits éducatifs.
        - **Contrôle logiciel total :** Les développeurs pouvaient manipuler l''afficheur au niveau électrique le plus fondamental.

        ### Limites

        - **Très inefficace :** Le CPU consacre une part importante de son temps à rafraîchir l''écran au lieu d''exécuter des applications.
        - **Faible évolutivité :** Dès que les écrans grandissaient ou devenaient plus complexes, l''approche n''était plus praticable.
        - **Capacité graphique limitée :** Adaptée uniquement à du texte simple ou à des motifs de pixels en basse résolution.

        ## Déclin et héritage

        Le contrôle d''affichage par logiciel a rapidement disparu dès que les contrôleurs vidéo dédiés sont devenus abordables. Ces puces pouvaient bufferiser les images, générer les signaux de cadencement et gérer automatiquement les cycles de rafraîchissement, libérant ainsi le CPU et autorisant des graphismes plus riches.

        Malgré sa courte existence, cette technique a joué un rôle crucial dans l''évolution précoce de l''informatique. Elle a permis l''apparition même de la première vague de micro-ordinateurs et de dispositifs embarqués bon marché, montrant tout ce qu''il était possible d''accomplir avec un matériel minimal et un logiciel astucieux.

        ## Résumé technique

        - **Définition :** Sortie d''affichage entièrement produite par des signaux électriques pilotés par le CPU.
        - **Besoins matériels :** LCD simple ou afficheur à segments ; ni GPU ni contrôleur vidéo.
        - **Charge CPU :** Très élevée ; rafraîchissement continu nécessaire.
        - **Usages typiques :** Calculatrices, premiers micro-ordinateurs, kits DIY, systèmes à coût réduit.
        - **Capacité graphique :** Minimale ; adaptée uniquement aux affichages basse résolution ou à segments.
        - **Remplacée par :** Contrôleurs vidéo dédiés et GPU intégrées.
        ', '<h1>Contrôle d''affichage par logiciel</h1>
        <h3>Quand un ordinateur n''a pas de GPU mais affiche tout de même une image</h3>
        <p>Aux premiers temps de l''informatique et dans les systèmes à très bas coût, certaines machines produisaient une sortie vidéo <strong>sans aucun processeur graphique dédié</strong>. À la place, le <strong>processeur principal</strong> était chargé de générer chaque pixel à l''écran. Cette technique — appelée <strong>contrôle d''affichage par logiciel</strong> — représente l''une des approches les plus minimalistes jamais utilisées en infographie.</p>
        <h2>Contexte historique</h2>
        <p>Avant que les contrôleurs vidéo abordables et les puces graphiques intégrées ne se généralisent, beaucoup de premiers micro-ordinateurs et dispositifs embarqués s''appuyaient sur la CPU pour commuter des lignes électriques directement reliées à un afficheur. Cette approche était particulièrement courante dans :</p>
        <ul><li><strong>Les calculatrices et instruments scientifiques</strong>, dont les écrans étaient petits et statiques.</li><li><strong>Les premiers micro-ordinateurs à LCD simples</strong>, souvent assemblés sous forme de kits pour amateurs ou de bricolage.</li><li><strong>Les systèmes à coût réduit</strong>, où la suppression d''une puce graphique abaissait significativement les coûts de fabrication.</li></ul>
        <p>À mesure que les résolutions augmentaient et que les interfaces graphiques se sophistiquaient, cette méthode a rapidement disparu. Au milieu des années 1980, le matériel vidéo dédié était devenu suffisamment abordable pour rendre les affichages pilotés par logiciel pratiquement obsolètes.</p>
        <h2>Fonctionnement</h2>
        <p>Dans un système sans GPU, le CPU doit gérer <strong>tous les aspects du dessin</strong> :</p>
        <ul><li><strong>Contrôle des pixels :</strong> Le processeur manipule directement les broches électriques correspondant aux lignes et colonnes de l''afficheur.</li><li><strong>Cadencement :</strong> Le CPU doit maintenir des cycles de rafraîchissement précis pour que l''image reste stable.</li><li><strong>Rendu :</strong> Tous les graphismes — texte, formes, animations — sont calculés et émis en temps réel par des routines logicielles.</li></ul>
        <p>Cette méthode n''est viable que lorsque l''écran comporte <strong>très peu de pixels</strong>. Un petit LCD monochrome, par exemple, peut ne nécessiter que quelques dizaines de lignes de contrôle, ce qui permet à la CPU de les gérer directement.</p>
        <h2>Avantages et limites</h2>
        <h3>Avantages</h3>
        <ul><li><strong>Coût extrêmement bas :</strong> Pas de puce graphique, pas de mémoire vidéo, circuiterie minimale.</li><li><strong>Simplicité :</strong> Idéale pour les premiers systèmes amateurs et les kits éducatifs.</li><li><strong>Contrôle logiciel total :</strong> Les développeurs pouvaient manipuler l''afficheur au niveau électrique le plus fondamental.</li></ul>
        <h3>Limites</h3>
        <ul><li><strong>Très inefficace :</strong> Le CPU consacre une part importante de son temps à rafraîchir l''écran au lieu d''exécuter des applications.</li><li><strong>Faible évolutivité :</strong> Dès que les écrans grandissaient ou devenaient plus complexes, l''approche n''était plus praticable.</li><li><strong>Capacité graphique limitée :</strong> Adaptée uniquement à du texte simple ou à des motifs de pixels en basse résolution.</li></ul>
        <h2>Déclin et héritage</h2>
        <p>Le contrôle d''affichage par logiciel a rapidement disparu dès que les contrôleurs vidéo dédiés sont devenus abordables. Ces puces pouvaient bufferiser les images, générer les signaux de cadencement et gérer automatiquement les cycles de rafraîchissement, libérant ainsi le CPU et autorisant des graphismes plus riches.</p>
        <p>Malgré sa courte existence, cette technique a joué un rôle crucial dans l''évolution précoce de l''informatique. Elle a permis l''apparition même de la première vague de micro-ordinateurs et de dispositifs embarqués bon marché, montrant tout ce qu''il était possible d''accomplir avec un matériel minimal et un logiciel astucieux.</p>
        <h2>Résumé technique</h2>
        <ul><li><strong>Définition :</strong> Sortie d''affichage entièrement produite par des signaux électriques pilotés par le CPU.</li><li><strong>Besoins matériels :</strong> LCD simple ou afficheur à segments ; ni GPU ni contrôleur vidéo.</li><li><strong>Charge CPU :</strong> Très élevée ; rafraîchissement continu nécessaire.</li><li><strong>Usages typiques :</strong> Calculatrices, premiers micro-ordinateurs, kits DIY, systèmes à coût réduit.</li><li><strong>Capacité graphique :</strong> Minimale ; adaptée uniquement aux affichages basse résolution ou à segments.</li><li><strong>Remplacée par :</strong> Contrôleurs vidéo dédiés et GPU intégrées.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -1 AND `LanguageCode` = 'fra'
            );
        ");

        // -------- ita --------
        migrationBuilder.Sql(@"
            INSERT INTO `GpuDescriptions` (`GpuId`, `LanguageCode`, `Text`, `Html`)
            SELECT -1, 'ita', '# Controllo del display via software
        ### Quando un computer non ha GPU ma disegna comunque a schermo

        Agli albori dell''informatica e nei sistemi a bassissimo costo, alcune macchine producevano un''uscita video **senza alcun processore grafico dedicato**. Al suo posto, il **processore principale** era responsabile della generazione di ogni pixel sul display. Questa tecnica — nota come **controllo del display via software** — rappresenta uno degli approcci più minimalisti mai utilizzati nella grafica al computer.

        ## Contesto storico

        Prima che i controller video a basso costo e i chip grafici integrati si diffondessero, molti primi microcomputer e dispositivi embedded si affidavano alla CPU per commutare linee elettriche collegate direttamente al display. Questo approccio era particolarmente diffuso in:

        - **Calcolatrici e strumenti scientifici**, con display piccoli e statici.
        - **Primi microcomputer con LCD semplici**, spesso assemblati come kit per appassionati o fai-da-te.
        - **Sistemi a costo ridotto**, in cui eliminare un chip grafico abbatteva notevolmente i costi di produzione.

        Con l''aumento delle risoluzioni e la crescente sofisticazione delle interfacce grafiche, questo metodo scomparve rapidamente. A metà degli anni Ottanta l''hardware video dedicato era diventato abbastanza economico da rendere i display pilotati via software sostanzialmente obsoleti.

        ## Come funziona

        In un sistema senza GPU, la CPU deve gestire **ogni aspetto del disegno**:

        - **Controllo dei pixel:** Il processore manipola direttamente i piedini elettrici corrispondenti alle righe e alle colonne del display.
        - **Temporizzazione:** La CPU deve mantenere cicli di refresh precisi affinché l''immagine resti stabile.
        - **Rendering:** Tutta la grafica — testo, forme, animazioni — viene calcolata ed emessa in tempo reale da routine software.

        Questo metodo è praticabile solo quando il display ha **pochissimi pixel**. Un piccolo LCD monocromatico, per esempio, può richiedere solo poche decine di linee di controllo, rendendo possibile la gestione diretta da parte della CPU.

        ## Vantaggi e limiti

        ### Vantaggi

        - **Costo estremamente basso:** Nessun chip grafico, nessuna memoria video, circuiteria minima.
        - **Semplicità:** Ideale per i primi sistemi per appassionati e per i kit didattici.
        - **Pieno controllo software:** Gli sviluppatori potevano manipolare il display al livello elettrico più basilare.

        ### Limiti

        - **Molto inefficiente:** La CPU dedica una buona parte del proprio tempo al refresh del display anziché all''esecuzione delle applicazioni.
        - **Scarsa scalabilità:** Non appena i display crescevano o diventavano più complessi, l''approccio non era più praticabile.
        - **Capacità grafica limitata:** Adatto solo a testo semplice o a pattern di pixel a bassa risoluzione.

        ## Declino ed eredità

        Il controllo del display via software svanì rapidamente non appena i controller video dedicati divennero accessibili. Questi chip potevano bufferizzare le immagini, generare i segnali di temporizzazione e gestire i cicli di refresh in modo automatico, liberando la CPU e abilitando una grafica più ricca.

        Nonostante la sua breve esistenza, questa tecnica ha giocato un ruolo cruciale nell''evoluzione iniziale dell''informatica. Ha consentito l''esistenza stessa della prima ondata di microcomputer e dispositivi embedded a basso costo, dimostrando quanto si potesse ottenere con hardware minimale e software ingegnoso.

        ## Riepilogo tecnico

        - **Definizione:** Uscita video prodotta interamente da segnali elettrici controllati dalla CPU.
        - **Requisiti hardware:** LCD semplice o display a segmenti; nessuna GPU né controller video.
        - **Carico CPU:** Molto elevato; refresh continuo richiesto.
        - **Uso tipico:** Calcolatrici, primi microcomputer, kit fai-da-te, sistemi a costo ridotto.
        - **Capacità grafica:** Minima; adatta solo a display a bassa risoluzione o a segmenti.
        - **Sostituito da:** Controller video dedicati e GPU integrate.
        ', '<h1>Controllo del display via software</h1>
        <h3>Quando un computer non ha GPU ma disegna comunque a schermo</h3>
        <p>Agli albori dell''informatica e nei sistemi a bassissimo costo, alcune macchine producevano un''uscita video <strong>senza alcun processore grafico dedicato</strong>. Al suo posto, il <strong>processore principale</strong> era responsabile della generazione di ogni pixel sul display. Questa tecnica — nota come <strong>controllo del display via software</strong> — rappresenta uno degli approcci più minimalisti mai utilizzati nella grafica al computer.</p>
        <h2>Contesto storico</h2>
        <p>Prima che i controller video a basso costo e i chip grafici integrati si diffondessero, molti primi microcomputer e dispositivi embedded si affidavano alla CPU per commutare linee elettriche collegate direttamente al display. Questo approccio era particolarmente diffuso in:</p>
        <ul><li><strong>Calcolatrici e strumenti scientifici</strong>, con display piccoli e statici.</li><li><strong>Primi microcomputer con LCD semplici</strong>, spesso assemblati come kit per appassionati o fai-da-te.</li><li><strong>Sistemi a costo ridotto</strong>, in cui eliminare un chip grafico abbatteva notevolmente i costi di produzione.</li></ul>
        <p>Con l''aumento delle risoluzioni e la crescente sofisticazione delle interfacce grafiche, questo metodo scomparve rapidamente. A metà degli anni Ottanta l''hardware video dedicato era diventato abbastanza economico da rendere i display pilotati via software sostanzialmente obsoleti.</p>
        <h2>Come funziona</h2>
        <p>In un sistema senza GPU, la CPU deve gestire <strong>ogni aspetto del disegno</strong>:</p>
        <ul><li><strong>Controllo dei pixel:</strong> Il processore manipola direttamente i piedini elettrici corrispondenti alle righe e alle colonne del display.</li><li><strong>Temporizzazione:</strong> La CPU deve mantenere cicli di refresh precisi affinché l''immagine resti stabile.</li><li><strong>Rendering:</strong> Tutta la grafica — testo, forme, animazioni — viene calcolata ed emessa in tempo reale da routine software.</li></ul>
        <p>Questo metodo è praticabile solo quando il display ha <strong>pochissimi pixel</strong>. Un piccolo LCD monocromatico, per esempio, può richiedere solo poche decine di linee di controllo, rendendo possibile la gestione diretta da parte della CPU.</p>
        <h2>Vantaggi e limiti</h2>
        <h3>Vantaggi</h3>
        <ul><li><strong>Costo estremamente basso:</strong> Nessun chip grafico, nessuna memoria video, circuiteria minima.</li><li><strong>Semplicità:</strong> Ideale per i primi sistemi per appassionati e per i kit didattici.</li><li><strong>Pieno controllo software:</strong> Gli sviluppatori potevano manipolare il display al livello elettrico più basilare.</li></ul>
        <h3>Limiti</h3>
        <ul><li><strong>Molto inefficiente:</strong> La CPU dedica una buona parte del proprio tempo al refresh del display anziché all''esecuzione delle applicazioni.</li><li><strong>Scarsa scalabilità:</strong> Non appena i display crescevano o diventavano più complessi, l''approccio non era più praticabile.</li><li><strong>Capacità grafica limitata:</strong> Adatto solo a testo semplice o a pattern di pixel a bassa risoluzione.</li></ul>
        <h2>Declino ed eredità</h2>
        <p>Il controllo del display via software svanì rapidamente non appena i controller video dedicati divennero accessibili. Questi chip potevano bufferizzare le immagini, generare i segnali di temporizzazione e gestire i cicli di refresh in modo automatico, liberando la CPU e abilitando una grafica più ricca.</p>
        <p>Nonostante la sua breve esistenza, questa tecnica ha giocato un ruolo cruciale nell''evoluzione iniziale dell''informatica. Ha consentito l''esistenza stessa della prima ondata di microcomputer e dispositivi embedded a basso costo, dimostrando quanto si potesse ottenere con hardware minimale e software ingegnoso.</p>
        <h2>Riepilogo tecnico</h2>
        <ul><li><strong>Definizione:</strong> Uscita video prodotta interamente da segnali elettrici controllati dalla CPU.</li><li><strong>Requisiti hardware:</strong> LCD semplice o display a segmenti; nessuna GPU né controller video.</li><li><strong>Carico CPU:</strong> Molto elevato; refresh continuo richiesto.</li><li><strong>Uso tipico:</strong> Calcolatrici, primi microcomputer, kit fai-da-te, sistemi a costo ridotto.</li><li><strong>Capacità grafica:</strong> Minima; adatta solo a display a bassa risoluzione o a segmenti.</li><li><strong>Sostituito da:</strong> Controller video dedicati e GPU integrate.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `GpuDescriptions`
                WHERE `GpuId` = -1 AND `LanguageCode` = 'ita'
            );
        ");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM `GpuDescriptions`
            WHERE `GpuId` = -1
            AND `LanguageCode` IN ('eng', 'spa', 'deu', 'fra', 'ita');
        ");
    }
}
