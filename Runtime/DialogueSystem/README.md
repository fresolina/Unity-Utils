# Dialogue System

Se till så det går skapa bra dialog-träd som assets. Det funkar inte att ha varje dialog-frame som object i scenen.
(Så småningom kanske vi kan använda Unity 6.3s nya GraphView API för att skapa ett visuellt editor-fönster)
Möjligen krävs att vi använda System.Guid för att länka mellan asset och scen-objekt.
    (Möjligen Skapa GuidManager, som runtime konverterar Guid till int-index i arrayen för alla objekt)

## TODO

Anpassa exemplet efter så få specialactions som möjligt.

Men testa även "Give Room" -> Starta en action som gör att inkeepern går iväg och hämtar en nyckel ur nyckelskåpet
och när han kommer tillbaka så fortsätter vi dialogen (kanske genom MultipleAction med flera seriella steg?)
Innkeeper.FetchKey
StartDialogue(give key)
Innkeeper.GiveKeyToPlayer
