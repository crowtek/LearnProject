Dialogue & Cutscene System (ScriptableObject Architecture)
Dieses Subsystem steuert die komplette Dialog logik des Spiels. 

Es wurde nach dem Sovereign Architecture-Prinzip entwickelt: 
Alle Kernkomponenten sind vollständig voneinander entkoppelt und kommunizieren ausschließlich über ereignisgesteuerte ScriptableObject-Channels (EventChannelSO).

🏗️ Systemübersicht & Datenfluss
Das System unterscheidet zwischen zwei Arten von Dialog-Auslösern:
Interaktive Dialoge: Der Spieler spricht aktiv mit einem NPC (NPC_Interactable).
Automatische Cutscene-Dialoge: Ein Dialog ploppt ohne Interaktion sofort auf, sobald ein bestimmtes StoryFlag im Spiel gecastet wird (StoryCutsceneTrigger).


🛠️ Die Kernkomponenten1. 
Datenhaltung & Editor-ToolingDialogueDatabaseSO.cs Die zentrale Text-Datenbank. 
Hält eine Liste von DialogueEntry-Strukturen (Key, Textzeilen, optionales ResultFlag). 
Zur Laufzeit wird die Liste in ein Dictionary umgewandelt (OnEnable), um Abfragen über den DialogueKey in $O(1)$-Zeit zu garantieren.

DialogueOptionDrawer.cs 
Ein CustomPropertyDrawer für das [DialogueKey]-Attribut. 
Er sucht automatisch nach der DialogueDatabaseSO im Projekt und stellt dem Spieldesigner im Inspector ein sicheres Drop-Down-Menü statt fehleranfälliger Strings zur Verfügung.

DialogueUIController.cs 
Verwaltet das UI Toolkit-Dokument (UIDocument). Reagiert auf den DialogueDataChannelSO, schaltet das Player-Input-System stumm (toggleInputChannel) und visualisiert den Sprechernamen sowie das Porträt.


🌟 Architektur-Highlights 
Keine harten Abhängigkeiten (Decoupled): 
Der UI-Controller weiß nichts von NPCs oder Story-Triggern. 
NPCs wissen nichts vom UI. Das macht den Austausch der UI-Präsentation (z.B. Wechsel von UI Toolkit zu URP 3D-Texten) möglich, ohne eine Zeile Logikcode anzufassen.
Design-First Workflow: Durch den Custom Property Drawer und die ScriptableObject-Datenbanken können neue Quests, Story-Flags und Dialogstränge komplett im Unity-Editor konfiguriert werden, ohne neuen C#-Code schreiben zu müssen.
Performance-Optimiert: String-Vergleiche und Listen-Iterationen wurden zur Laufzeit durch performante Dictionary-Lookups ($O(1)$) in den Datenbanken und ein HashSet für die abgeschlossenen Story-Flags in den NPCs ersetzt.