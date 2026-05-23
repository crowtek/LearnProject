📖 Mein Story- & Flag-System (Unity)
Es ist komplett eventgesteuert (Event-Driven), modular und nutzt die Power von ScriptableObjects, um Logik, Daten und visuelle Objekte sauber voneinander zu trennen.

Keine Komponente muss die andere kennen. Sie reden alle nur über Kanäle (Channels) miteinander. 
Und für das Team (oder mich selbst im Editor) gibt es ein custom Dropdown-Menü, damit Tippfehler bei den Flags der Vergangenheit angehören.

🛠️ Aus welchen Teilen besteht das System?
Das System teilt sich in vier kleine, hochspezialisierte Bereiche auf:

1. Der Kern & State (GlobalStoryStateSO & StoryManager)
GlobalStoryStateSO: Mein Datenspeicher. Ein ScriptableObject, das einfach eine Liste aller geschafften Meilensteine (completedFlags) hält. Es überlebt Szenenwechsel ohne Probleme.

StoryManager: Er lauscht auf Anfragen, um neue Flags zu setzen, speichert sie im State und ballert sie dann über den Output-Kanal raus, damit alle Systeme Bescheid wissen. 
Beim Szenenstart sorgt er dafür, dass bereits erfüllte Flags noch einmal gefeuert werden, damit die Welt synchron bleibt.

2. Die Trigger (StoryTrigger & DialogueManager)
Sie wissen nichts vom Manager. Wenn du durch einen StoryTrigger läufst oder einen Dialog beendest, wird das jeweilige Flag einfach als Event in den setFlagRequestChannel geworfen.

3. Die Welt-Reaktoren (ConditionCheckedObject & NPC_Interactable)
ConditionCheckedObject: Schaltet sich oder andere GameObjects (wie Barrieren, Quest-Gegenstände) automatisch ein oder aus, sobald ihr spezifisches Flag über den Äther läuft. 
Dank einer showWhenRight-Checkbox super flexibel für Designer.

NPC_Interactable: Ändert die Dialog-Prioritäten eines NPCs basierend auf dem aktuellen Story-Fortschritt.

🎨 Das Editor-Highlight: StoryFlagDrawer & StoryFlagDatabaseSO
Weil Strings in Unity die Hölle sind (ein Tippfehler und die Quest bricht!), habe ich ein kleines Tool für den Inspector geschrieben:
StoryFlagDatabaseSO: Eine zentrale Asset-Liste, in der einfach alle Story-Flags des Spiels als Text eingetragen werden (z.B. Story1, Quest_Cave_Cleared).
StoryFlagDrawer: Mein Custom PropertyDrawer. Sobald ich im Code [StoryFlag] über einen String schreibe, verwandelt das Editor-Skript das Textfeld im Unity-Inspector in ein sauberes Dropdown-Menü.
Es sucht sich automatisch die Datenbank, liest die Flags aus und stellt sie zur Auswahl bereit. Keine Tippfehler mehr, kein Suchen nach dem exakten Flag-Namen!

🚀 Warum ich das so gebaut habe (Die Philosophie)
Keine Singletons, keine harten Referenzen: Wenn ich eine Szene teste, muss ich nicht das halbe Spiel mitladen. Die Trigger werfen Events in den Kanal – ob jemand zuhört, ist ihnen egal.
Prefab-Friendly: Da alles über ScriptableObject-Kanäle läuft, kann ich Trigger und reaktive Objekte als Prefabs speichern. Ich muss keine Szenen-Referenzen von Hand im Inspector verknüpfen.
Workflow im Fokus: Spiele entstehen im Editor. Wenn das Tooling (Dropdowns statt Strings) stimmt, baut man weniger Bugs und das Level-Design macht dreimal so viel Spaß!