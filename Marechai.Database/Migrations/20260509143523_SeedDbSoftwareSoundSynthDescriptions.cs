using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Marechai.Database.Migrations;

/// <summary>
/// Seeds the description for the DB_SOFTWARE sentinel sound synthesizer
/// (id = -2, see <c>Marechai.Database.Operations.DbSoftware</c>) in all
/// five languages supported by the application UI: English (eng), Spanish
/// (spa), German (deu), French (fra) and Italian (ita).
///
/// The DB_SOFTWARE row itself is seeded by the legacy pre-EF migration
/// <c>UpdateDatabaseToV17</c> in <c>Marechai.Database/Operations/Update.cs</c>.
/// This migration only fills the `SoundSynthDescriptions` table for it.
///
/// Inserts are idempotent (gated by `WHERE NOT EXISTS`) so re-running the
/// migration on a DB where some or all rows already exist will not fail
/// with a duplicate-key error.
/// </summary>
public partial class SeedDbSoftwareSoundSynthDescriptions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // -------- eng --------
        migrationBuilder.Sql(@"
            INSERT INTO `SoundSynthDescriptions` (`SoundSynthId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'eng', '# Software-Driven Sound Synthesis

        In the history of personal computing, one of the most fundamental distinctions in audio design lies between **hardware-based sound generation** and **software-driven sound synthesis**. The latter refers to systems in which the computer''s main processor produces audio directly, without relying on a dedicated sound or music generation chip. Instead of writing to oscillators, envelope generators, or waveform engines, the software outputs numerical audio samples straight to a **DAC (digital-to-analog converter)** or, in the most minimal designs, directly to a speaker line.

        ## Historical Context

        Software-generated audio emerged early in the development of microcomputers, particularly in machines where cost constraints or design simplicity outweighed the need for advanced sound hardware. Many 1970s and early 1980s systems—especially those aimed at education or low-cost home use—implemented audio as little more than a toggled output pin. In these machines, all tonal variation, rhythm, and effects were produced by carefully timed CPU instructions.

        As semiconductor prices fell and integrated audio codecs became more affordable, this approach declined. By the 1990s, most mainstream systems incorporated dedicated audio hardware capable of buffering, mixing, and converting digital samples without occupying the CPU.

        ## How Software-Driven Synthesis Works

        In a software-only design, the CPU is responsible for **every aspect of sound generation**:

        - **Waveform creation:** The program computes the amplitude values of the desired sound wave—square, sawtooth, noise, or fully sampled audio.
        - **Timing:** The CPU must output samples at a stable rate (e.g., 8 kHz, 22 kHz, 44.1 kHz), often requiring cycle-accurate loops or timer interrupts.
        - **Output:** The computed values are written directly to a DAC register or toggled on a speaker line.

        This approach offers **complete flexibility**, since any waveform that can be computed can be played. However, it also places strict real-time demands on the processor.

        ## Advantages and Limitations

        Software-driven synthesis has historically been used for several reasons:

        - **Cost reduction:** Eliminating a sound chip reduced the bill of materials, especially in early home computers.
        - **Legal and licensing considerations:** Some manufacturers avoided patented sound-chip technologies by generating audio in software.
        - **Flexibility:** Developers could implement custom sound engines without being limited by fixed hardware capabilities.

        Yet the approach carries a significant drawback: **sound generation consumes CPU time that could otherwise be used by the main application or game**. Depending on the complexity of the audio routine, this could range from negligible overhead to a substantial performance penalty.

        The resulting sound quality varied widely. Some systems produced only simple ""beeps"" and ""boops"", while others—given sufficient CPU power and a quality DAC—could output audio approaching modern CD-quality fidelity.

        ## Decline and Legacy

        By the late 1980s and early 1990s, dedicated audio hardware became inexpensive enough that most systems adopted **codec chips** capable of handling sample buffering, mixing, and conversion autonomously. These devices freed the CPU from real-time audio duties and enabled richer, more consistent sound across platforms.

        Nevertheless, software-driven synthesis remains an important chapter in computing history. It illustrates the ingenuity of early developers who coaxed expressive audio from minimal hardware, and it continues to influence modern techniques such as procedural audio generation and CPU-based softsynths.

        ## Technical Summary

        - **Definition:** Audio generated entirely by CPU-executed software routines.
        - **Output Path:** Direct to DAC or speaker line.
        - **Hardware Requirements:** Minimal; often a single DAC or simple output pin.
        - **CPU Load:** Potentially high, depending on sample rate and waveform complexity.
        - **Sound Quality:** Ranges from simple tones to high-fidelity sampled audio.
        - **Common Use Cases:** Early microcomputers, cost-reduced systems, custom or experimental audio engines.
        - **Superseded By:** Dedicated audio codecs and integrated sound processors.
        ', '<h1>Software-Driven Sound Synthesis</h1>
        <p>In the history of personal computing, one of the most fundamental distinctions in audio design lies between <strong>hardware-based sound generation</strong> and <strong>software-driven sound synthesis</strong>. The latter refers to systems in which the computer''s main processor produces audio directly, without relying on a dedicated sound or music generation chip. Instead of writing to oscillators, envelope generators, or waveform engines, the software outputs numerical audio samples straight to a <strong>DAC (digital-to-analog converter)</strong> or, in the most minimal designs, directly to a speaker line.</p>
        <h2>Historical Context</h2>
        <p>Software-generated audio emerged early in the development of microcomputers, particularly in machines where cost constraints or design simplicity outweighed the need for advanced sound hardware. Many 1970s and early 1980s systems—especially those aimed at education or low-cost home use—implemented audio as little more than a toggled output pin. In these machines, all tonal variation, rhythm, and effects were produced by carefully timed CPU instructions.</p>
        <p>As semiconductor prices fell and integrated audio codecs became more affordable, this approach declined. By the 1990s, most mainstream systems incorporated dedicated audio hardware capable of buffering, mixing, and converting digital samples without occupying the CPU.</p>
        <h2>How Software-Driven Synthesis Works</h2>
        <p>In a software-only design, the CPU is responsible for <strong>every aspect of sound generation</strong>:</p>
        <ul><li><strong>Waveform creation:</strong> The program computes the amplitude values of the desired sound wave—square, sawtooth, noise, or fully sampled audio.</li><li><strong>Timing:</strong> The CPU must output samples at a stable rate (e.g., 8 kHz, 22 kHz, 44.1 kHz), often requiring cycle-accurate loops or timer interrupts.</li><li><strong>Output:</strong> The computed values are written directly to a DAC register or toggled on a speaker line.</li></ul>
        <p>This approach offers <strong>complete flexibility</strong>, since any waveform that can be computed can be played. However, it also places strict real-time demands on the processor.</p>
        <h2>Advantages and Limitations</h2>
        <p>Software-driven synthesis has historically been used for several reasons:</p>
        <ul><li><strong>Cost reduction:</strong> Eliminating a sound chip reduced the bill of materials, especially in early home computers.</li><li><strong>Legal and licensing considerations:</strong> Some manufacturers avoided patented sound-chip technologies by generating audio in software.</li><li><strong>Flexibility:</strong> Developers could implement custom sound engines without being limited by fixed hardware capabilities.</li></ul>
        <p>Yet the approach carries a significant drawback: <strong>sound generation consumes CPU time that could otherwise be used by the main application or game</strong>. Depending on the complexity of the audio routine, this could range from negligible overhead to a substantial performance penalty.</p>
        <p>The resulting sound quality varied widely. Some systems produced only simple ""beeps"" and ""boops"", while others—given sufficient CPU power and a quality DAC—could output audio approaching modern CD-quality fidelity.</p>
        <h2>Decline and Legacy</h2>
        <p>By the late 1980s and early 1990s, dedicated audio hardware became inexpensive enough that most systems adopted <strong>codec chips</strong> capable of handling sample buffering, mixing, and conversion autonomously. These devices freed the CPU from real-time audio duties and enabled richer, more consistent sound across platforms.</p>
        <p>Nevertheless, software-driven synthesis remains an important chapter in computing history. It illustrates the ingenuity of early developers who coaxed expressive audio from minimal hardware, and it continues to influence modern techniques such as procedural audio generation and CPU-based softsynths.</p>
        <h2>Technical Summary</h2>
        <ul><li><strong>Definition:</strong> Audio generated entirely by CPU-executed software routines.</li><li><strong>Output Path:</strong> Direct to DAC or speaker line.</li><li><strong>Hardware Requirements:</strong> Minimal; often a single DAC or simple output pin.</li><li><strong>CPU Load:</strong> Potentially high, depending on sample rate and waveform complexity.</li><li><strong>Sound Quality:</strong> Ranges from simple tones to high-fidelity sampled audio.</li><li><strong>Common Use Cases:</strong> Early microcomputers, cost-reduced systems, custom or experimental audio engines.</li><li><strong>Superseded By:</strong> Dedicated audio codecs and integrated sound processors.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `SoundSynthDescriptions`
                WHERE `SoundSynthId` = -2 AND `LanguageCode` = 'eng'
            );
        ");

        // -------- spa --------
        migrationBuilder.Sql(@"
            INSERT INTO `SoundSynthDescriptions` (`SoundSynthId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'spa', '# Síntesis de sonido por software

        En la historia de la informática personal, una de las distinciones más fundamentales en el diseño de audio se encuentra entre la **generación de sonido por hardware** y la **síntesis de sonido por software**. Esta última se refiere a sistemas en los que el procesador principal del ordenador produce audio directamente, sin recurrir a un chip dedicado de generación de sonido o música. En lugar de escribir en osciladores, generadores de envolvente o motores de formas de onda, el software envía muestras numéricas de audio directamente a un **DAC (convertidor digital a analógico)** o, en los diseños más minimalistas, directamente a una línea de altavoz.

        ## Contexto histórico

        El audio generado por software apareció pronto en el desarrollo de los microordenadores, sobre todo en máquinas donde las restricciones de coste o la simplicidad de diseño primaban sobre la necesidad de un hardware de sonido avanzado. Muchos sistemas de los años setenta y principios de los ochenta —especialmente los destinados a la educación o al uso doméstico de bajo coste— implementaron el audio como poco más que un pin de salida conmutado. En estas máquinas, toda variación tonal, ritmo y efecto se producía mediante instrucciones de CPU cuidadosamente temporizadas.

        A medida que los precios de los semiconductores cayeron y los códecs de audio integrados se hicieron más asequibles, este enfoque fue declinando. Hacia los años noventa, la mayoría de los sistemas convencionales incorporaban hardware de audio dedicado capaz de almacenar en búfer, mezclar y convertir muestras digitales sin ocupar la CPU.

        ## Cómo funciona la síntesis por software

        En un diseño puramente por software, la CPU es responsable de **todos los aspectos de la generación de sonido**:

        - **Creación de la forma de onda:** El programa calcula los valores de amplitud de la onda deseada —cuadrada, diente de sierra, ruido o audio totalmente muestreado.
        - **Temporización:** La CPU debe emitir las muestras a una tasa estable (por ejemplo, 8 kHz, 22 kHz, 44,1 kHz), lo que a menudo requiere bucles con precisión de ciclo o interrupciones de temporizador.
        - **Salida:** Los valores calculados se escriben directamente en un registro DAC o se conmutan en una línea de altavoz.

        Este enfoque ofrece **flexibilidad total**, ya que puede reproducirse cualquier forma de onda que pueda calcularse. Sin embargo, también impone exigencias estrictas de tiempo real al procesador.

        ## Ventajas y limitaciones

        La síntesis por software se ha utilizado históricamente por varias razones:

        - **Reducción de coste:** Eliminar un chip de sonido reducía la lista de materiales, especialmente en los primeros ordenadores domésticos.
        - **Consideraciones legales y de licencias:** Algunos fabricantes evitaban tecnologías patentadas de chips de sonido generando audio por software.
        - **Flexibilidad:** Los desarrolladores podían implementar motores de sonido a medida sin las limitaciones de un hardware fijo.

        Sin embargo, este enfoque tiene un inconveniente significativo: **la generación de sonido consume tiempo de CPU que podría dedicarse a la aplicación o al juego principal**. Según la complejidad de la rutina de audio, esto podía oscilar entre una sobrecarga insignificante y una penalización sustancial del rendimiento.

        La calidad de sonido resultante variaba mucho. Algunos sistemas producían solo simples ""pitidos"" y ""tonos"", mientras que otros —con suficiente potencia de CPU y un DAC de calidad— podían generar audio cercano a la fidelidad moderna de un CD.

        ## Declive y legado

        A finales de los años ochenta y principios de los noventa, el hardware de audio dedicado se abarató lo suficiente como para que la mayoría de los sistemas adoptaran **chips códec** capaces de gestionar el almacenamiento en búfer, la mezcla y la conversión de muestras de forma autónoma. Estos dispositivos liberaron a la CPU de las tareas de audio en tiempo real y permitieron un sonido más rico y coherente entre plataformas.

        No obstante, la síntesis por software sigue siendo un capítulo importante de la historia de la informática. Ilustra el ingenio de los primeros desarrolladores, que arrancaron audio expresivo de un hardware mínimo, y sigue influyendo en técnicas modernas como la generación de audio procedimental y los softsynths basados en CPU.

        ## Resumen técnico

        - **Definición:** Audio generado íntegramente mediante rutinas de software ejecutadas por la CPU.
        - **Camino de salida:** Directo al DAC o a una línea de altavoz.
        - **Requisitos de hardware:** Mínimos; a menudo un único DAC o un sencillo pin de salida.
        - **Carga de CPU:** Potencialmente alta, según la frecuencia de muestreo y la complejidad de la forma de onda.
        - **Calidad de sonido:** Va desde tonos simples hasta audio muestreado de alta fidelidad.
        - **Casos de uso habituales:** Primeros microordenadores, sistemas de coste reducido, motores de audio personalizados o experimentales.
        - **Sustituida por:** Códecs de audio dedicados y procesadores de sonido integrados.
        ', '<h1>Síntesis de sonido por software</h1>
        <p>En la historia de la informática personal, una de las distinciones más fundamentales en el diseño de audio se encuentra entre la <strong>generación de sonido por hardware</strong> y la <strong>síntesis de sonido por software</strong>. Esta última se refiere a sistemas en los que el procesador principal del ordenador produce audio directamente, sin recurrir a un chip dedicado de generación de sonido o música. En lugar de escribir en osciladores, generadores de envolvente o motores de formas de onda, el software envía muestras numéricas de audio directamente a un <strong>DAC (convertidor digital a analógico)</strong> o, en los diseños más minimalistas, directamente a una línea de altavoz.</p>
        <h2>Contexto histórico</h2>
        <p>El audio generado por software apareció pronto en el desarrollo de los microordenadores, sobre todo en máquinas donde las restricciones de coste o la simplicidad de diseño primaban sobre la necesidad de un hardware de sonido avanzado. Muchos sistemas de los años setenta y principios de los ochenta —especialmente los destinados a la educación o al uso doméstico de bajo coste— implementaron el audio como poco más que un pin de salida conmutado. En estas máquinas, toda variación tonal, ritmo y efecto se producía mediante instrucciones de CPU cuidadosamente temporizadas.</p>
        <p>A medida que los precios de los semiconductores cayeron y los códecs de audio integrados se hicieron más asequibles, este enfoque fue declinando. Hacia los años noventa, la mayoría de los sistemas convencionales incorporaban hardware de audio dedicado capaz de almacenar en búfer, mezclar y convertir muestras digitales sin ocupar la CPU.</p>
        <h2>Cómo funciona la síntesis por software</h2>
        <p>En un diseño puramente por software, la CPU es responsable de <strong>todos los aspectos de la generación de sonido</strong>:</p>
        <ul><li><strong>Creación de la forma de onda:</strong> El programa calcula los valores de amplitud de la onda deseada —cuadrada, diente de sierra, ruido o audio totalmente muestreado.</li><li><strong>Temporización:</strong> La CPU debe emitir las muestras a una tasa estable (por ejemplo, 8 kHz, 22 kHz, 44,1 kHz), lo que a menudo requiere bucles con precisión de ciclo o interrupciones de temporizador.</li><li><strong>Salida:</strong> Los valores calculados se escriben directamente en un registro DAC o se conmutan en una línea de altavoz.</li></ul>
        <p>Este enfoque ofrece <strong>flexibilidad total</strong>, ya que puede reproducirse cualquier forma de onda que pueda calcularse. Sin embargo, también impone exigencias estrictas de tiempo real al procesador.</p>
        <h2>Ventajas y limitaciones</h2>
        <p>La síntesis por software se ha utilizado históricamente por varias razones:</p>
        <ul><li><strong>Reducción de coste:</strong> Eliminar un chip de sonido reducía la lista de materiales, especialmente en los primeros ordenadores domésticos.</li><li><strong>Consideraciones legales y de licencias:</strong> Algunos fabricantes evitaban tecnologías patentadas de chips de sonido generando audio por software.</li><li><strong>Flexibilidad:</strong> Los desarrolladores podían implementar motores de sonido a medida sin las limitaciones de un hardware fijo.</li></ul>
        <p>Sin embargo, este enfoque tiene un inconveniente significativo: <strong>la generación de sonido consume tiempo de CPU que podría dedicarse a la aplicación o al juego principal</strong>. Según la complejidad de la rutina de audio, esto podía oscilar entre una sobrecarga insignificante y una penalización sustancial del rendimiento.</p>
        <p>La calidad de sonido resultante variaba mucho. Algunos sistemas producían solo simples ""pitidos"" y ""tonos"", mientras que otros —con suficiente potencia de CPU y un DAC de calidad— podían generar audio cercano a la fidelidad moderna de un CD.</p>
        <h2>Declive y legado</h2>
        <p>A finales de los años ochenta y principios de los noventa, el hardware de audio dedicado se abarató lo suficiente como para que la mayoría de los sistemas adoptaran <strong>chips códec</strong> capaces de gestionar el almacenamiento en búfer, la mezcla y la conversión de muestras de forma autónoma. Estos dispositivos liberaron a la CPU de las tareas de audio en tiempo real y permitieron un sonido más rico y coherente entre plataformas.</p>
        <p>No obstante, la síntesis por software sigue siendo un capítulo importante de la historia de la informática. Ilustra el ingenio de los primeros desarrolladores, que arrancaron audio expresivo de un hardware mínimo, y sigue influyendo en técnicas modernas como la generación de audio procedimental y los softsynths basados en CPU.</p>
        <h2>Resumen técnico</h2>
        <ul><li><strong>Definición:</strong> Audio generado íntegramente mediante rutinas de software ejecutadas por la CPU.</li><li><strong>Camino de salida:</strong> Directo al DAC o a una línea de altavoz.</li><li><strong>Requisitos de hardware:</strong> Mínimos; a menudo un único DAC o un sencillo pin de salida.</li><li><strong>Carga de CPU:</strong> Potencialmente alta, según la frecuencia de muestreo y la complejidad de la forma de onda.</li><li><strong>Calidad de sonido:</strong> Va desde tonos simples hasta audio muestreado de alta fidelidad.</li><li><strong>Casos de uso habituales:</strong> Primeros microordenadores, sistemas de coste reducido, motores de audio personalizados o experimentales.</li><li><strong>Sustituida por:</strong> Códecs de audio dedicados y procesadores de sonido integrados.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `SoundSynthDescriptions`
                WHERE `SoundSynthId` = -2 AND `LanguageCode` = 'spa'
            );
        ");

        // -------- deu --------
        migrationBuilder.Sql(@"
            INSERT INTO `SoundSynthDescriptions` (`SoundSynthId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'deu', '# Software-gesteuerte Klangsynthese

        In der Geschichte des Personal Computing besteht eine der grundlegendsten Unterscheidungen im Audiodesign zwischen **hardwarebasierter Klangerzeugung** und **softwaregesteuerter Klangsynthese**. Letztere bezeichnet Systeme, in denen der Hauptprozessor des Computers das Audiosignal direkt erzeugt, ohne auf einen speziellen Klang- oder Musikchip zurückzugreifen. Anstatt Oszillatoren, Hüllkurvengeneratoren oder Wellenform-Engines anzusteuern, gibt die Software numerische Audiosamples direkt an einen **DAC (Digital-Analog-Wandler)** aus oder, in den minimalsten Konstruktionen, unmittelbar an eine Lautsprecherleitung.

        ## Historischer Kontext

        Softwaregenerierter Klang trat schon früh in der Entwicklung der Mikrocomputer auf, insbesondere bei Maschinen, in denen Kostenbeschränkungen oder Designvereinfachung wichtiger waren als der Bedarf an fortschrittlicher Audiohardware. Viele Systeme der 1970er- und frühen 1980er-Jahre — vor allem solche für Bildung oder den preiswerten Heimgebrauch — realisierten Audio kaum mehr als über einen umgeschalteten Ausgangspin. Bei diesen Maschinen wurde jede tonale Variation, jeder Rhythmus und jeder Effekt durch sorgfältig getaktete CPU-Befehle erzeugt.

        Mit dem Sinken der Halbleiterpreise und der zunehmenden Verfügbarkeit integrierter Audiocodecs verlor dieser Ansatz an Bedeutung. In den 1990er-Jahren verfügten die meisten gängigen Systeme über dedizierte Audiohardware, die digitale Samples puffern, mischen und wandeln konnte, ohne die CPU zu belasten.

        ## Funktionsweise

        In einem reinen Software-Design ist die CPU für **jeden Aspekt der Klangerzeugung** verantwortlich:

        - **Wellenformerzeugung:** Das Programm berechnet die Amplitudenwerte der gewünschten Schwingung — Rechteck, Sägezahn, Rauschen oder vollständig gesampeltes Audio.
        - **Zeitsteuerung:** Die CPU muss Samples in einer stabilen Rate ausgeben (z. B. 8 kHz, 22 kHz, 44,1 kHz), was häufig zyklusgenaue Schleifen oder Timer-Interrupts erfordert.
        - **Ausgabe:** Die berechneten Werte werden direkt in ein DAC-Register geschrieben oder auf einer Lautsprecherleitung umgeschaltet.

        Dieser Ansatz bietet **vollständige Flexibilität**, da jede berechenbare Wellenform abgespielt werden kann. Er stellt jedoch auch strenge Echtzeitanforderungen an den Prozessor.

        ## Vorteile und Grenzen

        Softwaregesteuerte Synthese wurde aus mehreren Gründen historisch eingesetzt:

        - **Kostenreduktion:** Der Verzicht auf einen Klangchip senkte die Stückliste, besonders bei frühen Heimcomputern.
        - **Rechtliche und lizenzrechtliche Erwägungen:** Einige Hersteller umgingen patentierte Klangchip-Technologien, indem sie Audio per Software erzeugten.
        - **Flexibilität:** Entwickler konnten eigene Sound-Engines implementieren, ohne durch feste Hardwarefähigkeiten eingeschränkt zu sein.

        Der Ansatz hat jedoch einen erheblichen Nachteil: **Die Klangerzeugung verbraucht CPU-Zeit, die sonst für die eigentliche Anwendung oder das Spiel zur Verfügung stünde**. Je nach Komplexität der Audioroutine konnte dies von vernachlässigbarem Overhead bis zu einer deutlichen Leistungseinbuße reichen.

        Die resultierende Klangqualität variierte stark. Einige Systeme erzeugten nur einfache „Pieps""- und „Boop""-Töne, während andere — bei ausreichender CPU-Leistung und einem hochwertigen DAC — Audio nahezu in moderner CD-Qualität ausgeben konnten.

        ## Niedergang und Erbe

        Ende der 1980er- und Anfang der 1990er-Jahre war dedizierte Audiohardware so erschwinglich geworden, dass die meisten Systeme **Codec-Chips** einsetzten, die das Puffern, Mischen und Wandeln von Samples eigenständig erledigten. Diese Bausteine entlasteten die CPU von Echtzeit-Audioaufgaben und ermöglichten reicheren und plattformübergreifend konsistenteren Klang.

        Dennoch bleibt die softwaregesteuerte Synthese ein bedeutendes Kapitel der Computergeschichte. Sie veranschaulicht den Erfindungsreichtum früher Entwickler, die ausdrucksstarken Klang aus minimaler Hardware herauskitzelten, und sie beeinflusst weiterhin moderne Techniken wie prozedurale Audioerzeugung und CPU-basierte Softsynths.

        ## Technische Zusammenfassung

        - **Definition:** Audio, das vollständig durch CPU-ausgeführte Softwareroutinen erzeugt wird.
        - **Ausgabeweg:** Direkt zum DAC oder zur Lautsprecherleitung.
        - **Hardwareanforderungen:** Minimal; oft ein einzelner DAC oder ein einfacher Ausgangspin.
        - **CPU-Last:** Potenziell hoch, abhängig von Abtastrate und Wellenformkomplexität.
        - **Klangqualität:** Reicht von einfachen Tönen bis zu hochauflösendem Sample-Audio.
        - **Typische Einsatzfälle:** Frühe Mikrocomputer, kostenreduzierte Systeme, eigene oder experimentelle Audio-Engines.
        - **Abgelöst durch:** Dedizierte Audio-Codecs und integrierte Soundprozessoren.
        ', '<h1>Software-gesteuerte Klangsynthese</h1>
        <p>In der Geschichte des Personal Computing besteht eine der grundlegendsten Unterscheidungen im Audiodesign zwischen <strong>hardwarebasierter Klangerzeugung</strong> und <strong>softwaregesteuerter Klangsynthese</strong>. Letztere bezeichnet Systeme, in denen der Hauptprozessor des Computers das Audiosignal direkt erzeugt, ohne auf einen speziellen Klang- oder Musikchip zurückzugreifen. Anstatt Oszillatoren, Hüllkurvengeneratoren oder Wellenform-Engines anzusteuern, gibt die Software numerische Audiosamples direkt an einen <strong>DAC (Digital-Analog-Wandler)</strong> aus oder, in den minimalsten Konstruktionen, unmittelbar an eine Lautsprecherleitung.</p>
        <h2>Historischer Kontext</h2>
        <p>Softwaregenerierter Klang trat schon früh in der Entwicklung der Mikrocomputer auf, insbesondere bei Maschinen, in denen Kostenbeschränkungen oder Designvereinfachung wichtiger waren als der Bedarf an fortschrittlicher Audiohardware. Viele Systeme der 1970er- und frühen 1980er-Jahre — vor allem solche für Bildung oder den preiswerten Heimgebrauch — realisierten Audio kaum mehr als über einen umgeschalteten Ausgangspin. Bei diesen Maschinen wurde jede tonale Variation, jeder Rhythmus und jeder Effekt durch sorgfältig getaktete CPU-Befehle erzeugt.</p>
        <p>Mit dem Sinken der Halbleiterpreise und der zunehmenden Verfügbarkeit integrierter Audiocodecs verlor dieser Ansatz an Bedeutung. In den 1990er-Jahren verfügten die meisten gängigen Systeme über dedizierte Audiohardware, die digitale Samples puffern, mischen und wandeln konnte, ohne die CPU zu belasten.</p>
        <h2>Funktionsweise</h2>
        <p>In einem reinen Software-Design ist die CPU für <strong>jeden Aspekt der Klangerzeugung</strong> verantwortlich:</p>
        <ul><li><strong>Wellenformerzeugung:</strong> Das Programm berechnet die Amplitudenwerte der gewünschten Schwingung — Rechteck, Sägezahn, Rauschen oder vollständig gesampeltes Audio.</li><li><strong>Zeitsteuerung:</strong> Die CPU muss Samples in einer stabilen Rate ausgeben (z. B. 8 kHz, 22 kHz, 44,1 kHz), was häufig zyklusgenaue Schleifen oder Timer-Interrupts erfordert.</li><li><strong>Ausgabe:</strong> Die berechneten Werte werden direkt in ein DAC-Register geschrieben oder auf einer Lautsprecherleitung umgeschaltet.</li></ul>
        <p>Dieser Ansatz bietet <strong>vollständige Flexibilität</strong>, da jede berechenbare Wellenform abgespielt werden kann. Er stellt jedoch auch strenge Echtzeitanforderungen an den Prozessor.</p>
        <h2>Vorteile und Grenzen</h2>
        <p>Softwaregesteuerte Synthese wurde aus mehreren Gründen historisch eingesetzt:</p>
        <ul><li><strong>Kostenreduktion:</strong> Der Verzicht auf einen Klangchip senkte die Stückliste, besonders bei frühen Heimcomputern.</li><li><strong>Rechtliche und lizenzrechtliche Erwägungen:</strong> Einige Hersteller umgingen patentierte Klangchip-Technologien, indem sie Audio per Software erzeugten.</li><li><strong>Flexibilität:</strong> Entwickler konnten eigene Sound-Engines implementieren, ohne durch feste Hardwarefähigkeiten eingeschränkt zu sein.</li></ul>
        <p>Der Ansatz hat jedoch einen erheblichen Nachteil: <strong>Die Klangerzeugung verbraucht CPU-Zeit, die sonst für die eigentliche Anwendung oder das Spiel zur Verfügung stünde</strong>. Je nach Komplexität der Audioroutine konnte dies von vernachlässigbarem Overhead bis zu einer deutlichen Leistungseinbuße reichen.</p>
        <p>Die resultierende Klangqualität variierte stark. Einige Systeme erzeugten nur einfache „Pieps""- und „Boop""-Töne, während andere — bei ausreichender CPU-Leistung und einem hochwertigen DAC — Audio nahezu in moderner CD-Qualität ausgeben konnten.</p>
        <h2>Niedergang und Erbe</h2>
        <p>Ende der 1980er- und Anfang der 1990er-Jahre war dedizierte Audiohardware so erschwinglich geworden, dass die meisten Systeme <strong>Codec-Chips</strong> einsetzten, die das Puffern, Mischen und Wandeln von Samples eigenständig erledigten. Diese Bausteine entlasteten die CPU von Echtzeit-Audioaufgaben und ermöglichten reicheren und plattformübergreifend konsistenteren Klang.</p>
        <p>Dennoch bleibt die softwaregesteuerte Synthese ein bedeutendes Kapitel der Computergeschichte. Sie veranschaulicht den Erfindungsreichtum früher Entwickler, die ausdrucksstarken Klang aus minimaler Hardware herauskitzelten, und sie beeinflusst weiterhin moderne Techniken wie prozedurale Audioerzeugung und CPU-basierte Softsynths.</p>
        <h2>Technische Zusammenfassung</h2>
        <ul><li><strong>Definition:</strong> Audio, das vollständig durch CPU-ausgeführte Softwareroutinen erzeugt wird.</li><li><strong>Ausgabeweg:</strong> Direkt zum DAC oder zur Lautsprecherleitung.</li><li><strong>Hardwareanforderungen:</strong> Minimal; oft ein einzelner DAC oder ein einfacher Ausgangspin.</li><li><strong>CPU-Last:</strong> Potenziell hoch, abhängig von Abtastrate und Wellenformkomplexität.</li><li><strong>Klangqualität:</strong> Reicht von einfachen Tönen bis zu hochauflösendem Sample-Audio.</li><li><strong>Typische Einsatzfälle:</strong> Frühe Mikrocomputer, kostenreduzierte Systeme, eigene oder experimentelle Audio-Engines.</li><li><strong>Abgelöst durch:</strong> Dedizierte Audio-Codecs und integrierte Soundprozessoren.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `SoundSynthDescriptions`
                WHERE `SoundSynthId` = -2 AND `LanguageCode` = 'deu'
            );
        ");

        // -------- fra --------
        migrationBuilder.Sql(@"
            INSERT INTO `SoundSynthDescriptions` (`SoundSynthId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'fra', '# Synthèse sonore logicielle

        Dans l''histoire de l''informatique personnelle, l''une des distinctions les plus fondamentales en matière de conception audio se situe entre la **génération sonore matérielle** et la **synthèse sonore logicielle**. Cette dernière désigne les systèmes dans lesquels le processeur principal de l''ordinateur produit l''audio directement, sans recourir à une puce dédiée à la génération de son ou de musique. Au lieu d''écrire dans des oscillateurs, des générateurs d''enveloppe ou des moteurs de formes d''onde, le logiciel envoie des échantillons audio numériques directement à un **CAN (convertisseur numérique-analogique)** ou, dans les conceptions les plus minimalistes, directement à une ligne de haut-parleur.

        ## Contexte historique

        L''audio généré par logiciel est apparu tôt dans le développement des micro-ordinateurs, en particulier sur les machines où les contraintes de coût ou la simplicité de conception primaient sur le besoin d''un matériel sonore avancé. Beaucoup de systèmes des années 1970 et du début des années 1980 — notamment ceux destinés à l''éducation ou à un usage domestique économique — implémentaient l''audio à peine plus qu''à l''aide d''une broche de sortie commutée. Sur ces machines, toute variation tonale, tout rythme et tout effet étaient produits par des instructions CPU soigneusement cadencées.

        À mesure que le prix des semiconducteurs baissait et que les codecs audio intégrés devenaient plus abordables, cette approche déclinait. Dès les années 1990, la plupart des systèmes courants intégraient un matériel audio dédié capable de tamponner, mélanger et convertir des échantillons numériques sans solliciter le CPU.

        ## Fonctionnement

        Dans une conception purement logicielle, le CPU est responsable de **tous les aspects de la génération sonore** :

        - **Création de la forme d''onde :** Le programme calcule les valeurs d''amplitude de l''onde souhaitée — carrée, en dents de scie, bruit ou audio entièrement échantillonné.
        - **Cadencement :** Le CPU doit produire les échantillons à un débit stable (par exemple 8 kHz, 22 kHz, 44,1 kHz), ce qui exige souvent des boucles à la cycle près ou des interruptions de minuterie.
        - **Sortie :** Les valeurs calculées sont écrites directement dans un registre du CAN ou commutées sur une ligne de haut-parleur.

        Cette approche offre une **flexibilité totale**, puisque toute forme d''onde calculable peut être jouée. Elle impose toutefois des contraintes temps réel strictes au processeur.

        ## Avantages et limites

        La synthèse logicielle a historiquement été utilisée pour plusieurs raisons :

        - **Réduction des coûts :** L''élimination d''une puce sonore réduisait la nomenclature, surtout sur les premiers ordinateurs domestiques.
        - **Considérations juridiques et de licence :** Certains fabricants évitaient les technologies de puces sonores brevetées en générant l''audio par logiciel.
        - **Flexibilité :** Les développeurs pouvaient implémenter des moteurs sonores sur mesure sans être limités par des capacités matérielles fixes.

        Cette approche présente cependant un inconvénient majeur : **la génération sonore consomme du temps CPU qui pourrait sinon servir à l''application ou au jeu principal**. Selon la complexité de la routine audio, cela pouvait aller d''une charge négligeable à une pénalité de performance importante.

        La qualité sonore obtenue variait beaucoup. Certains systèmes ne produisaient que de simples « bips » et « boops », tandis que d''autres — avec une puissance CPU suffisante et un CAN de qualité — pouvaient sortir un audio proche de la fidélité moderne du CD.

        ## Déclin et héritage

        À la fin des années 1980 et au début des années 1990, le matériel audio dédié était devenu suffisamment abordable pour que la plupart des systèmes adoptent des **puces codec** capables de gérer le tamponnage, le mixage et la conversion des échantillons de manière autonome. Ces composants ont libéré le CPU des tâches audio en temps réel et ont permis un son plus riche et plus cohérent entre plateformes.

        La synthèse logicielle reste néanmoins un chapitre important de l''histoire de l''informatique. Elle illustre l''ingéniosité des premiers développeurs, qui tiraient un son expressif d''un matériel minimal, et elle continue d''influencer les techniques modernes telles que la génération audio procédurale et les softsynths exécutés sur CPU.

        ## Résumé technique

        - **Définition :** Audio entièrement produit par des routines logicielles exécutées par le CPU.
        - **Chemin de sortie :** Directement vers un CAN ou une ligne de haut-parleur.
        - **Besoins matériels :** Minimes ; souvent un unique CAN ou une simple broche de sortie.
        - **Charge CPU :** Potentiellement élevée, selon la fréquence d''échantillonnage et la complexité de la forme d''onde.
        - **Qualité sonore :** Va de simples tonalités à un audio échantillonné en haute fidélité.
        - **Cas d''usage courants :** Premiers micro-ordinateurs, systèmes à coût réduit, moteurs audio personnalisés ou expérimentaux.
        - **Remplacée par :** Codecs audio dédiés et processeurs sonores intégrés.
        ', '<h1>Synthèse sonore logicielle</h1>
        <p>Dans l''histoire de l''informatique personnelle, l''une des distinctions les plus fondamentales en matière de conception audio se situe entre la <strong>génération sonore matérielle</strong> et la <strong>synthèse sonore logicielle</strong>. Cette dernière désigne les systèmes dans lesquels le processeur principal de l''ordinateur produit l''audio directement, sans recourir à une puce dédiée à la génération de son ou de musique. Au lieu d''écrire dans des oscillateurs, des générateurs d''enveloppe ou des moteurs de formes d''onde, le logiciel envoie des échantillons audio numériques directement à un <strong>CAN (convertisseur numérique-analogique)</strong> ou, dans les conceptions les plus minimalistes, directement à une ligne de haut-parleur.</p>
        <h2>Contexte historique</h2>
        <p>L''audio généré par logiciel est apparu tôt dans le développement des micro-ordinateurs, en particulier sur les machines où les contraintes de coût ou la simplicité de conception primaient sur le besoin d''un matériel sonore avancé. Beaucoup de systèmes des années 1970 et du début des années 1980 — notamment ceux destinés à l''éducation ou à un usage domestique économique — implémentaient l''audio à peine plus qu''à l''aide d''une broche de sortie commutée. Sur ces machines, toute variation tonale, tout rythme et tout effet étaient produits par des instructions CPU soigneusement cadencées.</p>
        <p>À mesure que le prix des semiconducteurs baissait et que les codecs audio intégrés devenaient plus abordables, cette approche déclinait. Dès les années 1990, la plupart des systèmes courants intégraient un matériel audio dédié capable de tamponner, mélanger et convertir des échantillons numériques sans solliciter le CPU.</p>
        <h2>Fonctionnement</h2>
        <p>Dans une conception purement logicielle, le CPU est responsable de <strong>tous les aspects de la génération sonore</strong> :</p>
        <ul><li><strong>Création de la forme d''onde :</strong> Le programme calcule les valeurs d''amplitude de l''onde souhaitée — carrée, en dents de scie, bruit ou audio entièrement échantillonné.</li><li><strong>Cadencement :</strong> Le CPU doit produire les échantillons à un débit stable (par exemple 8 kHz, 22 kHz, 44,1 kHz), ce qui exige souvent des boucles à la cycle près ou des interruptions de minuterie.</li><li><strong>Sortie :</strong> Les valeurs calculées sont écrites directement dans un registre du CAN ou commutées sur une ligne de haut-parleur.</li></ul>
        <p>Cette approche offre une <strong>flexibilité totale</strong>, puisque toute forme d''onde calculable peut être jouée. Elle impose toutefois des contraintes temps réel strictes au processeur.</p>
        <h2>Avantages et limites</h2>
        <p>La synthèse logicielle a historiquement été utilisée pour plusieurs raisons :</p>
        <ul><li><strong>Réduction des coûts :</strong> L''élimination d''une puce sonore réduisait la nomenclature, surtout sur les premiers ordinateurs domestiques.</li><li><strong>Considérations juridiques et de licence :</strong> Certains fabricants évitaient les technologies de puces sonores brevetées en générant l''audio par logiciel.</li><li><strong>Flexibilité :</strong> Les développeurs pouvaient implémenter des moteurs sonores sur mesure sans être limités par des capacités matérielles fixes.</li></ul>
        <p>Cette approche présente cependant un inconvénient majeur : <strong>la génération sonore consomme du temps CPU qui pourrait sinon servir à l''application ou au jeu principal</strong>. Selon la complexité de la routine audio, cela pouvait aller d''une charge négligeable à une pénalité de performance importante.</p>
        <p>La qualité sonore obtenue variait beaucoup. Certains systèmes ne produisaient que de simples « bips » et « boops », tandis que d''autres — avec une puissance CPU suffisante et un CAN de qualité — pouvaient sortir un audio proche de la fidélité moderne du CD.</p>
        <h2>Déclin et héritage</h2>
        <p>À la fin des années 1980 et au début des années 1990, le matériel audio dédié était devenu suffisamment abordable pour que la plupart des systèmes adoptent des <strong>puces codec</strong> capables de gérer le tamponnage, le mixage et la conversion des échantillons de manière autonome. Ces composants ont libéré le CPU des tâches audio en temps réel et ont permis un son plus riche et plus cohérent entre plateformes.</p>
        <p>La synthèse logicielle reste néanmoins un chapitre important de l''histoire de l''informatique. Elle illustre l''ingéniosité des premiers développeurs, qui tiraient un son expressif d''un matériel minimal, et elle continue d''influencer les techniques modernes telles que la génération audio procédurale et les softsynths exécutés sur CPU.</p>
        <h2>Résumé technique</h2>
        <ul><li><strong>Définition :</strong> Audio entièrement produit par des routines logicielles exécutées par le CPU.</li><li><strong>Chemin de sortie :</strong> Directement vers un CAN ou une ligne de haut-parleur.</li><li><strong>Besoins matériels :</strong> Minimes ; souvent un unique CAN ou une simple broche de sortie.</li><li><strong>Charge CPU :</strong> Potentiellement élevée, selon la fréquence d''échantillonnage et la complexité de la forme d''onde.</li><li><strong>Qualité sonore :</strong> Va de simples tonalités à un audio échantillonné en haute fidélité.</li><li><strong>Cas d''usage courants :</strong> Premiers micro-ordinateurs, systèmes à coût réduit, moteurs audio personnalisés ou expérimentaux.</li><li><strong>Remplacée par :</strong> Codecs audio dédiés et processeurs sonores intégrés.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `SoundSynthDescriptions`
                WHERE `SoundSynthId` = -2 AND `LanguageCode` = 'fra'
            );
        ");

        // -------- ita --------
        migrationBuilder.Sql(@"
            INSERT INTO `SoundSynthDescriptions` (`SoundSynthId`, `LanguageCode`, `Text`, `Html`)
            SELECT -2, 'ita', '# Sintesi sonora via software

        Nella storia dell''informatica personale, una delle distinzioni più fondamentali nella progettazione audio si colloca tra la **generazione sonora basata su hardware** e la **sintesi sonora via software**. Quest''ultima indica i sistemi in cui il processore principale del computer produce l''audio direttamente, senza ricorrere a un chip dedicato alla generazione di suoni o di musica. Anziché scrivere su oscillatori, generatori di inviluppo o motori di forme d''onda, il software invia campioni audio numerici direttamente a un **DAC (convertitore digitale-analogico)** o, nelle realizzazioni più essenziali, direttamente a una linea dell''altoparlante.

        ## Contesto storico

        L''audio generato via software comparve presto nello sviluppo dei microcomputer, soprattutto nelle macchine in cui i vincoli di costo o la semplicità di progetto avevano la precedenza sull''esigenza di hardware sonoro avanzato. Molti sistemi degli anni Settanta e dei primi anni Ottanta — in particolare quelli destinati all''istruzione o all''uso domestico a basso costo — implementavano l''audio poco più di un piedino d''uscita commutato. In queste macchine ogni variazione tonale, ritmo ed effetto era prodotto da istruzioni della CPU temporizzate con cura.

        Con il calo dei prezzi dei semiconduttori e la maggiore disponibilità di codec audio integrati, questo approccio iniziò a declinare. Negli anni Novanta la maggior parte dei sistemi di consumo integrava hardware audio dedicato capace di bufferizzare, miscelare e convertire campioni digitali senza occupare la CPU.

        ## Come funziona

        In un progetto puramente software, la CPU è responsabile di **ogni aspetto della generazione del suono**:

        - **Creazione della forma d''onda:** Il programma calcola i valori di ampiezza dell''onda desiderata — quadra, dente di sega, rumore o audio completamente campionato.
        - **Temporizzazione:** La CPU deve emettere i campioni a un ritmo stabile (ad es. 8 kHz, 22 kHz, 44,1 kHz), cosa che spesso richiede cicli con precisione di clock o interruzioni di timer.
        - **Uscita:** I valori calcolati vengono scritti direttamente in un registro del DAC o commutati su una linea dell''altoparlante.

        Questo approccio offre **piena flessibilità**, poiché può riprodurre qualunque forma d''onda calcolabile. Tuttavia impone anche stringenti requisiti di tempo reale al processore.

        ## Vantaggi e limiti

        La sintesi via software è stata storicamente utilizzata per diversi motivi:

        - **Riduzione dei costi:** L''eliminazione di un chip sonoro riduceva la distinta dei materiali, soprattutto nei primi home computer.
        - **Considerazioni legali e di licenza:** Alcuni produttori evitavano tecnologie di chip sonori brevettati generando l''audio via software.
        - **Flessibilità:** Gli sviluppatori potevano implementare motori sonori personalizzati senza i limiti di un hardware prefissato.

        L''approccio ha però uno svantaggio significativo: **la generazione del suono consuma tempo di CPU che altrimenti potrebbe essere usato dall''applicazione o dal gioco principale**. A seconda della complessità della routine audio, l''overhead poteva variare da trascurabile a una sostanziale penalizzazione delle prestazioni.

        La qualità sonora risultante variava molto. Alcuni sistemi producevano solo semplici ""bip"" e ""boop"", mentre altri — con sufficiente potenza di CPU e un DAC di qualità — potevano emettere audio vicino alla moderna fedeltà di un CD.

        ## Declino ed eredità

        Tra la fine degli anni Ottanta e l''inizio degli anni Novanta l''hardware audio dedicato divenne abbastanza economico da spingere la maggior parte dei sistemi ad adottare **chip codec** in grado di gestire autonomamente il buffering, la miscelazione e la conversione dei campioni. Questi componenti liberarono la CPU dai compiti audio in tempo reale e consentirono un suono più ricco e più coerente tra piattaforme.

        Tuttavia la sintesi via software resta un capitolo importante della storia dell''informatica. Illustra l''ingegnosità dei primi sviluppatori, che riuscirono a trarre audio espressivo da hardware minimale, e continua a influenzare tecniche moderne come la generazione audio procedurale e i softsynth basati su CPU.

        ## Riepilogo tecnico

        - **Definizione:** Audio prodotto interamente da routine software eseguite dalla CPU.
        - **Percorso di uscita:** Diretto al DAC o a una linea dell''altoparlante.
        - **Requisiti hardware:** Minimi; spesso un singolo DAC o un semplice piedino di uscita.
        - **Carico CPU:** Potenzialmente elevato, in funzione della frequenza di campionamento e della complessità della forma d''onda.
        - **Qualità sonora:** Va da toni semplici ad audio campionato in alta fedeltà.
        - **Casi d''uso comuni:** Primi microcomputer, sistemi a costo ridotto, motori audio personalizzati o sperimentali.
        - **Sostituita da:** Codec audio dedicati e processori sonori integrati.
        ', '<h1>Sintesi sonora via software</h1>
        <p>Nella storia dell''informatica personale, una delle distinzioni più fondamentali nella progettazione audio si colloca tra la <strong>generazione sonora basata su hardware</strong> e la <strong>sintesi sonora via software</strong>. Quest''ultima indica i sistemi in cui il processore principale del computer produce l''audio direttamente, senza ricorrere a un chip dedicato alla generazione di suoni o di musica. Anziché scrivere su oscillatori, generatori di inviluppo o motori di forme d''onda, il software invia campioni audio numerici direttamente a un <strong>DAC (convertitore digitale-analogico)</strong> o, nelle realizzazioni più essenziali, direttamente a una linea dell''altoparlante.</p>
        <h2>Contesto storico</h2>
        <p>L''audio generato via software comparve presto nello sviluppo dei microcomputer, soprattutto nelle macchine in cui i vincoli di costo o la semplicità di progetto avevano la precedenza sull''esigenza di hardware sonoro avanzato. Molti sistemi degli anni Settanta e dei primi anni Ottanta — in particolare quelli destinati all''istruzione o all''uso domestico a basso costo — implementavano l''audio poco più di un piedino d''uscita commutato. In queste macchine ogni variazione tonale, ritmo ed effetto era prodotto da istruzioni della CPU temporizzate con cura.</p>
        <p>Con il calo dei prezzi dei semiconduttori e la maggiore disponibilità di codec audio integrati, questo approccio iniziò a declinare. Negli anni Novanta la maggior parte dei sistemi di consumo integrava hardware audio dedicato capace di bufferizzare, miscelare e convertire campioni digitali senza occupare la CPU.</p>
        <h2>Come funziona</h2>
        <p>In un progetto puramente software, la CPU è responsabile di <strong>ogni aspetto della generazione del suono</strong>:</p>
        <ul><li><strong>Creazione della forma d''onda:</strong> Il programma calcola i valori di ampiezza dell''onda desiderata — quadra, dente di sega, rumore o audio completamente campionato.</li><li><strong>Temporizzazione:</strong> La CPU deve emettere i campioni a un ritmo stabile (ad es. 8 kHz, 22 kHz, 44,1 kHz), cosa che spesso richiede cicli con precisione di clock o interruzioni di timer.</li><li><strong>Uscita:</strong> I valori calcolati vengono scritti direttamente in un registro del DAC o commutati su una linea dell''altoparlante.</li></ul>
        <p>Questo approccio offre <strong>piena flessibilità</strong>, poiché può riprodurre qualunque forma d''onda calcolabile. Tuttavia impone anche stringenti requisiti di tempo reale al processore.</p>
        <h2>Vantaggi e limiti</h2>
        <p>La sintesi via software è stata storicamente utilizzata per diversi motivi:</p>
        <ul><li><strong>Riduzione dei costi:</strong> L''eliminazione di un chip sonoro riduceva la distinta dei materiali, soprattutto nei primi home computer.</li><li><strong>Considerazioni legali e di licenza:</strong> Alcuni produttori evitavano tecnologie di chip sonori brevettati generando l''audio via software.</li><li><strong>Flessibilità:</strong> Gli sviluppatori potevano implementare motori sonori personalizzati senza i limiti di un hardware prefissato.</li></ul>
        <p>L''approccio ha però uno svantaggio significativo: <strong>la generazione del suono consuma tempo di CPU che altrimenti potrebbe essere usato dall''applicazione o dal gioco principale</strong>. A seconda della complessità della routine audio, l''overhead poteva variare da trascurabile a una sostanziale penalizzazione delle prestazioni.</p>
        <p>La qualità sonora risultante variava molto. Alcuni sistemi producevano solo semplici ""bip"" e ""boop"", mentre altri — con sufficiente potenza di CPU e un DAC di qualità — potevano emettere audio vicino alla moderna fedeltà di un CD.</p>
        <h2>Declino ed eredità</h2>
        <p>Tra la fine degli anni Ottanta e l''inizio degli anni Novanta l''hardware audio dedicato divenne abbastanza economico da spingere la maggior parte dei sistemi ad adottare <strong>chip codec</strong> in grado di gestire autonomamente il buffering, la miscelazione e la conversione dei campioni. Questi componenti liberarono la CPU dai compiti audio in tempo reale e consentirono un suono più ricco e più coerente tra piattaforme.</p>
        <p>Tuttavia la sintesi via software resta un capitolo importante della storia dell''informatica. Illustra l''ingegnosità dei primi sviluppatori, che riuscirono a trarre audio espressivo da hardware minimale, e continua a influenzare tecniche moderne come la generazione audio procedurale e i softsynth basati su CPU.</p>
        <h2>Riepilogo tecnico</h2>
        <ul><li><strong>Definizione:</strong> Audio prodotto interamente da routine software eseguite dalla CPU.</li><li><strong>Percorso di uscita:</strong> Diretto al DAC o a una linea dell''altoparlante.</li><li><strong>Requisiti hardware:</strong> Minimi; spesso un singolo DAC o un semplice piedino di uscita.</li><li><strong>Carico CPU:</strong> Potenzialmente elevato, in funzione della frequenza di campionamento e della complessità della forma d''onda.</li><li><strong>Qualità sonora:</strong> Va da toni semplici ad audio campionato in alta fedeltà.</li><li><strong>Casi d''uso comuni:</strong> Primi microcomputer, sistemi a costo ridotto, motori audio personalizzati o sperimentali.</li><li><strong>Sostituita da:</strong> Codec audio dedicati e processori sonori integrati.</li></ul>'
            FROM dual
            WHERE NOT EXISTS (
                SELECT 1 FROM `SoundSynthDescriptions`
                WHERE `SoundSynthId` = -2 AND `LanguageCode` = 'ita'
            );
        ");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM `SoundSynthDescriptions`
            WHERE `SoundSynthId` = -2
            AND `LanguageCode` IN ('eng', 'spa', 'deu', 'fra', 'ita');
        ");
    }
}
