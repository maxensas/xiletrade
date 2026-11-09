---
layout: default
title: Xiletrade
lang: de-DE
locale: de_DE
minidesc: PoE & PoE2 Preischeck und Overlay
description: Item-Preisprüfer und Overlay für Path of Exile und Path of Exile 2
strings:
  maintained: projekt wird gepflegt von
  generated: Diese Seite wurde generiert von
  download: Herunterladen
  portable: Portabel (.7z)
  installer: Installer (.exe)
  alt_customsearch: Fenster der benutzerdefinierten Suche von Xiletrade mit Suchformular, einer Voreinstellung, der Gegenstandskategorie und Stat-Filtern mit Vorschlägen
  alt_history: Währungsverlauf-Fenster von Xiletrade mit Diagrammen zu Volumen und Wechselkurs der ausgewählten Währung
  alt_searchresult: Ergebnisse der benutzerdefinierten Suche von Xiletrade mit Preisen, Gegenstandsstufe, Alter des Angebots und Verkäufern sowie dem Tooltip eines Gegenstands
  alt_settings_common: Xiletrade-Einstellungen, Registerkarte allgemeine Tasten mit den Tastenkürzeln der Funktionen
  alt_settings_additional: Xiletrade-Einstellungen, Registerkarte zusätzliche Tasten mit Chatbefehlen und Flüster-Tastenkürzeln
  repository: Auf GitHub anzeigen
  footer: Kostenloser Service, der für seine ordnungsgemäße Funktion fortlaufende Wartung erfordert.
  issue: und verwenden
  latest: neueste Version
  support: Spenden
---
{% include screenshot.html %}
## Sie den Preis Ihres Artikels

<h2 class="keywords"><i>🔍 Finde sofort die beste Preisspanne.</i></h2>
<h2 class="keywords"><i>⚡ Verhalte dich flexibel nach Bedarf.</i></h2>
<h2 class="keywords"><i>🎮 Spiele flüssiger, handle schneller!</i></h2>

#### {% include youtube.svg %} &nbsp; [Kurzübersicht ansehen](https://youtu.be/NygCZvZyUX0)<br>

Sehr geringe CPU-Auslastung.  
Durchschnittlich 170 MB RAM-Nutzung, ohne Spitzen.  
Keine Daten werden im Hintergrund geschrieben.  

* * *
<br><br><br>

### {% include direction.svg %} Kurze Einführung

<h2 class="keywords"><strong>Xiletrade bietet eine minimalistische Benutzeroberfläche für eine optimale Integration mit Path of Exile 1 und 2.</strong></h2>
<section class="intro">
  <p>Dank seiner Funktionen verbessert es Ihre Lebensqualität beim Spielen in Handelsligen und sorgt für ein reibungsloses Erlebnis.</p>
  <p>Als Open-Source erhält seine Codebasis sorgfältige Aufmerksamkeit, um Ihnen das bestmöglich optimierte Werkzeug bereitzustellen. 
  Unten finden Sie die vollständige Liste der anpassbaren Verhaltensweisen zur Preisprüfung von Gegenständen in POE.</p>
</section>

### {% include knife.svg %} Wichtigste Fakten

- Plattform: **Windows** 64-Bit, **portables** Archiv oder **Web-Installer** (keine Administratorrechte erforderlich).
- Kosten und Lizenz: kostenlos und Open Source (GPLv3), [Quellcode auf GitHub]({{ site.github.repository_url }}).
- Unterstützte Spiele: Path of Exile und Path of Exile 2.

### {% include pokeball.svg %} Hauptfunktionen

- Schneller **Preisprüfer** für alle Artikel mit der definierten Tastenkombination ***STRG+D*** *standardmäßig*.
- Öffnen Sie die **Einstellungen** schnell mit ***STRG+R***, um das Preisprüfungsverhalten zu ändern.
- **Massenartikelaustausch** für schnelle Flüstergeschäfte über POE.
- **Add-ons** zur Unterstützung bei der Preisgestaltung von Artikeln und zum Abrufen weiterer Informationen zu verwandten Artikeln.
	- Soft-Links mit **poe.prices**, **poeninja**, **poewiki** und **poedb**.
- Sucht nach **neuen Updates** und aktualisiert benutzerdefinierte/offizielle **Filter** beim Start.
- Funktionen, die die **Einzelaktionsregel** aus der [Richtlinie Dritter](https://www.pathofexile.com/developer/docs#policy) einhalten:
	- Interagieren Sie mit der **Chat-Konsole**, indem Sie vordefinierte Nachrichten mit zugehörigem **Hotkey** senden, z. B. schnelles Reisen zu **Verstecken** oder **Handelsaktionen**.
- Verknüpfen des **Mausrads** mit **Linksklick** auf Anfrage.
- Dienstprogramm **Regex-Manager**.
	- Verwalten Sie eine definierte Liste von **regulären Ausdrücken**.
	- Automatisches Einfügen in die Markierungsleiste.  
- **Währungsverlauf**, der die aktuellen Markttrends der ausgewählten Währung zeigt.
- Dienstprogramm **Benutzerdefinierte Suche** mit **Voreinstellungen** und Benachrichtigungen der **Live-Suche**.

{% include history.html %}

{% include regex.html %}
<br>

### {% include adjust.svg %} Dienstprogramm für benutzerdefinierte Suche

<h2 class="keywords"><i>🧭 Handelssuchen leicht gemacht, direkt über Ihrem Spiel.</i></h2>

Eine **minimalistische Oberfläche** mit allem Nötigen, standardmäßig mit ***STRG+E*** geöffnet.

- **Suchformular** mit einer Ergebnisliste.
	- Führen Sie eine einfache Suche durch, genau wie auf der Handelsseite oder im Spiel: Sie werden sich sofort zurechtfinden.
	- Name oder Basistyp des Gegenstands, Kategorie, Seltenheit, Filter, Sofortkaufpreis und **Stat-Gruppen**.
- **Verwaltung der Voreinstellungen**.
	- Speichern Sie Ihre Suchen, **exportieren** und **importieren** Sie sie, um sie mit Freunden zu teilen.
	- Die Kompatibilität mit **kommenden Ligen** ist gewährleistet: Voreinstellungen werden geprüft und repariert, wenn sich die Handelsfilter ändern.
- **Live-Suche**, nur für **fortgeschrittene Benutzer**, während Sie spielen.
	- **Mehrere Live-Suchen**, die Sie pausieren, fortsetzen oder beenden können, auch bei geschlossenem Fenster.
	- Neue Gegenstände werden als **Benachrichtigungen** angezeigt, mit vielen Aktionen zum Entdecken:  
zum Versteck des Verkäufers reisen, schließen oder Gegenstände **anheften**, auf die Sie nicht sofort antworten können.

{% include customsearch.html %}

Die Live-Suche ist **von Grund auf sicher** und hält Sie bei GGG in gutem Ansehen:

- **Gesicherte Anfragen**: Die von der Handels-API gesendeten Ratenlimits werden eingehalten, sie wird nie über ihre Regeln hinaus abgefragt, so werden **Timeouts vermieden**.
- **Abruf-Cooldown**: Neue Gegenstände werden mit einer Wartezeit zwischen den Anfragen abgerufen, nur die aktuellsten Ergebnisse erreichen Ihre Benachrichtigungen.
- **Optimiert für den Preisprüfer**: Live-Suchen nutzen nur einen Teil der Anfragelimits, Ihre **Preisprüfungen behalten Vorrang**.
- **Auto-Pause**: Eine Suche mit zu vielen Ergebnissen wird pausiert, **verfeinern** Sie ihre Kriterien und setzen Sie sie dann fort.
- **Keine Automatisierung**: Um zum Versteck eines Verkäufers zu gelangen, ist immer ein Klick auf die Benachrichtigung nötig.

<br>


### {% include install.svg %} Installation und Einrichtung

1. [**Laden Sie**]({% include installer.html %}) den neuesten **Web-Installer** herunter und starten Sie ihn *(keine Administratorrechte erforderlich)*,  
oder [**laden Sie**]({% include download.html %}) die **portable** Version herunter und **entpacken** Sie das **`.7z`**-Archiv in das gewünschte Verzeichnis.  
2. **Rechtsklick** auf das Taskleistensymbol, um das Programm zu konfigurieren oder zu schließen.
3. Wählen Sie im Start- oder Einstellungsfenster die gewünschte **Spielversion**, **Sprache** und **Liga** aus.
4. Bewegen Sie den Mauszeiger über jede Funktion, bis ein Tooltip erscheint, um eine ausführliche Erklärung zu erhalten.  
<br>
{% include settings.html %}
<br>

* * *
> ### {% include mouse.svg %} So funktioniert es
>
> Führen Sie Ihr Spiel im **Fenstermodus** oder **randlosen Vollbildmodus** aus, damit es richtig funktioniert.
> Stellen Sie sicher, dass die **Sprache** und **Liga** von Xiletrade mit den entsprechenden Spieleinstellungen übereinstimmen.
> Sobald Xiletrade gestartet und eingerichtet ist, können Sie jetzt die **Preise prüfen**, indem Sie diesen Vorgang befolgen:
> 1. Bewegen Sie Ihre Maus **über einen Artikel** im Spiel und drücken Sie ***STRG+D*** *(Standard)*
> 2. Es werden **Artikelinfobeschreibungen** aus dem Spiel kopiert und das Xiletrade-Fenster geöffnet.
> 3. Das Fenster zeigt **geschätzte Preise** an, wenn die Suche passende Ergebnisse liefert.
> 4. Die angezeigten Preise basieren auf den **offiziellen Handelswebsites** [PoE 1](https://www.pathofexile.com/trade/search/) / [PoE 2](https://www.pathofexile.com/trade2/search/poe2/).
<p class="accordion"><b>Details zur Preisüberprüfung anzeigen</b></p>
<div class="panel"> Beachten Sie, dass das Spiel den Fokus haben muss, da sonst das Tastenkürzel nicht funktioniert.
<br><br> Wenn Sie beispielsweise das Xiletrade-Fenster geöffnet lassen, darauf klicken (sodass Xiletrade den Fokus erhält) und anschließend eine weitere Preisüberprüfung versuchen, wird dies nicht funktionieren. Sie können jedoch mehrere Gegenstände nacheinander überprüfen, solange Sie nicht auf das Xiletrade-Fenster klicken.
<br><br> Jedes Mal, wenn PoE den Fokus verliert, werden die Tastenkürzel automatisch deregistriert und erneut registriert, sobald PoE den Fokus wieder erhält.
<br><br> Achten Sie außerdem darauf, nicht zu viele Preisüberprüfungen hintereinander durchzuführen, um eine temporäre Sperre der PoE-Handelswebsite zu vermeiden. Das Tool warnt Sie, wenn Sie seine Limits überschreiten, und schützt Sie vor einer massiven Zeitüberschreitung.
</div>

### {% include chip.svg %} Anpassbares Verhalten

* **Ziehen Sie das Hauptfenster** an die gewünschte Stelle auf Ihrem Bildschirm.
* Suchen Sie nach **aktuellen** Artikelwerten oder nach **Mindestwert** im Stufenbereich.
* Ändern Sie numerische Werte mit dem **Mausrad** (min./max.)
* Halten Sie die **STRG**- oder **UMSCHALT**-Taste für Dezimalwerte gedrückt.
* Passen Sie die **Deckkraft** und **Automatisches Schließen** an, wenn das Fenster den Fokus verliert.
* Klicken Sie auf das Symbol in der **oberen linken Ecke** des Hauptfensters.
* **Hervorhebung** für **teure** und **gefährliche Mods** in Karten (in der Konfigurationsdatei).
* **Automatisches Einfügen** von Whisper-Trades im Spiel von externen Websites.

```
Entwickelt, um die von der offiziellen Handelswebsite festgelegten Regeln einzuhalten,
um Missbrauch mit eingeschränkter Datenwiederherstellung und Anfragen im Laufe der Zeit zu vermeiden.
```
* * *

### {% include finger.svg %} FAQ

<p class="accordion"><b>Unterstützt die Anwendung alle Sprachen?</b></p>
<div class="panel">
<b>Ja</b>, sie wurde so konzipiert, dass sie alle Clientsprachen von Path of Exile unterstützt.
</div>

<p class="accordion"><b>Ist es sicher, die Anwendung von einer anderen Website herunterzuladen?</b></p>
<div class="panel">
Aus offensichtlichen Sicherheitsgründen wird dringend empfohlen, Xiletrade vom Haupt-Repository oder der zugehörigen Website github.io herunterzuladen.
</div>

<p class="accordion"><b>Ich weiß, dass die Verwendung einer Drittanbieter-App zu einem Ausschluss aus einem Spiel führen kann.<br>Ist dies bei Path of Exile mit Xiletrade der Fall?</b></p>
<div class="panel">
<b>Nein</b>, Xiletrade wurde erstmals im Jahr 2020 veröffentlicht und solange Sie die auf dieser Website veröffentlichte Version verwenden, besteht für Sie nie die Gefahr, gesperrt zu werden, sofern die GGG-Richtlinien die Verwendung solcher Tools zulassen.
<br>Beachten Sie, dass der Spieleherausgeber niemals eine Garantie für ein Drittanbieter-Tool übernimmt.
Es liegt in der Verantwortung des Projektbetreuers, sicherzustellen, dass das vorgeschlagene Tool den Regeln entspricht.
<br>Ich lade Sie ein, die <a target="_blank" rel="noopener noreferrer" href="https://www.pathofexile.com/developer/docs#policy">Nutzungsbedingungen</a> zu lesen, um die Informationen zu vergleichen.
</div>

<p class="accordion"><b>Warum ist die Software kostenlos?</b></p>
<div class="panel">
Xiletrade ist eine <b>eigenständige Software</b> und nicht dazu gedacht, durch die übliche Einbindung von Werbung Profit zu machen. Es ist jedoch möglich, nach eigenem Ermessen <a target="_blank" rel="noopener noreferrer" href="{{ site.github.paypal_url }}">Spenden</a> zu leisten. Wenn Ihnen das Projekt gefällt, sind Beiträge willkommen und wir danken Ihnen dafür 😊.
</div>