# Interaction System

## TODO

* Fixa så att Interactions-resursen har ett API för att hämta interactions, typ:
  * InteractionSystem.GetAllInteractions (GET /interactions) => All interactions
  * InteractionSystem.GetAllInteractions(onObject: Interactable) (GET /interactions/filter=onObject&objectId=123) => All interactions on the given object
  * InteractionSystem.GetValidInteractions(onObject: Interactable, withObject: Interactable) (GET /interactions/filter=onObject,withObject&objectId=123&withObjectId=456) => The valid interactions between the two objects, or null if none exists
  * InteractionSystem.GetAllInteractions()
* Sätt upp en Example-scene.
* Se till så InteractionSystem kan ladda in alla IInteraction-resurser i projektet vid runtime, så libbet kan använda projekt-specifika skapade interactions.
* Se till så libbet passar med VR (så UI:et går anpassa för VR. Templates?)
