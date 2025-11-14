Plan działania – MOBaPadMapper2
1. Cel projektu

Aplikacja MOBaPadMapper2 ma umożliwić granie w gry mobilne (szczególnie MOBA) przy użyciu gamepada, poprzez:

odczyt wejścia z kontrolera (przyciski, analogi),

zamianę go na dotykowe akcje na ekranie (tap, przytrzymanie, swipe, „aim & release”),

możliwość tworzenia wielu profili dopasowanych do różnych gier,

wygodny ekran konfiguracji, gdzie użytkownik graficznie ustawia położenie i działanie „przycisków dotykowych”.

Docelowo użytkownik powinien:

Wybrać profil dla gry (np. „Wild Rift”, „Mobile Legends”, „MOBA domyślny”).

Skonfigurować rozkład przycisków/umiejętności na ekranie.

Uruchomić grę.

Grać padem, podczas gdy aplikacja w tle emuluje dotyk w grze.

2. Architektura wysokopoziomowa
2.1. Warstwa UI (MAUI)

Rola: konfigurator i panel kontroli.

Elementy:

MainPage

Lista profili (GameProfile).

Informacja o aktywnym profilu.

Przycisk „Konfiguracja przycisków” otwierający ekran konfiguracji.

Nasłuchiwanie na gamepad (do testów w aplikacji i w przyszłości – info diagnostyczne).

TestPage (ekran konfiguracji profilu)

Duża powierzchnia testowa – wizualne rozmieszczenie przycisków.

Draggable kolorowe „bąble” odpowiadające przyciskom pada (A/B/X/Y itd.).

Panel konfiguracji:

typ akcji (Tap / Swipe / HoldAndAim / w przyszłości inne),

rozmiar przycisku,

opis / nazwa przycisku.

Reakcja na wciśnięcia gamepada:

wciśnięcie przycisku na padzie = zaznaczenie odpowiadającego przycisku na ekranie.

UI ma służyć tylko do konfiguracji i podglądu, nie musi być aktywne podczas samej gry.

2.2. Warstwa logiki mapowania

Modele:

GameProfile

Name

List<ActionMapping> Mappings

ActionMapping

GamepadButton TriggerButton

ActionType ActionType (Tap / Swipe / HoldAndAim / itd.)

double TargetX, TargetY (0..1 – pozycja na ekranie)

opcjonalnie: TargetX2, TargetY2 (dla swipe), Size (rozmiar bąbla)

MobaInputMapper

Przyjmuje GamepadState + rozmiar ekranu (screenWidth, screenHeight).

Dla każdego ActionMapping:

Tap → pojedynczy dotyk (AccessibilityService / TouchInjector).

Swipe → gest od (x1, y1) do (x2, y2).

HoldAndAim → przy przytrzymaniu przycisku start celowania, po puszczeniu – swipe zgodnie z kierunkiem drążka.

Ma metodę:

UpdateMappings(IEnumerable<ActionMapping> mappings) – aktualizowana przy zmianie profilu / modyfikacji na TestPage.

2.3. Warstwa wejścia z gamepada

GamepadModels:

GamepadButton – enum przycisków (A, B, X, Y, LB, RB, LT, RT, LeftStick, RightStick, Start, Back…).

StickState – X, Y w zakresie [-1..1].

GamepadState

HashSet<GamepadButton> PressedButtons

StickState LeftStick

StickState RightStick

IGamepadInputService / GamepadInputService

Singleton, który:

nasłuchuje zdarzeń z gamepada (Android KeyEvents / InputDevice / inna metoda),

składa je w GamepadState,

wywołuje event EventHandler<GamepadState> GamepadUpdated.

Źródła subskrypcji:

MainPage (testowanie i granie „wewnątrz” aplikacji),

w przyszłości: Androidowa warstwa runtime, która będzie działać kiedy gra jest na wierzchu.

2.4. Warstwa wstrzykiwania dotyku (Android)

ITouchInjector

TapAsync(x, y)

SwipeAsync(startX, startY, endX, endY, duration)

AndroidTouchInjector

Implementacja ITouchInjector.

Pod spodem odwołuje się do:

TouchAccessibilityService.Instance.PerformTapAsync(...)

TouchAccessibilityService.Instance.PerformSwipeAsync(...).

TouchAccessibilityService (Android AccessibilityService)

Skonfigurowany w AndroidManifest.xml + osobny plik XML (accessibilityservice_config).

Wykorzystuje AccessibilityService#dispatchGesture do wysyłania gestów do innych aplikacji.

Ma być zdolny do:

pojedynczego tapnięcia,

dłuższego tapnięcia,

swipe (gest liniowy),

w przyszłości: złożonych gestów (drag joysticka).

Warstwa ta ma działać gdy inna aplikacja (gra) jest na ekranie.

3. Roadmap – etapy pracy
Etap 0 – Stabilizacja obecnego stanu (który już prawie jest)

Cel: aplikacja uruchamia się, można:

wybrać profil na MainPage,

wejść w ekran konfiguracji,

przesuwać „bąble” i zmieniać ich rozmiar/typ akcji,

reagować na przyciski gamepada na TestPage (zaznaczanie).

Zadania:

 Ujednolicić namespace (wszędzie MOBaPadMapper2).

 Ustabilizować GameProfile, ActionMapping, MobaInputMapper, GamepadModels.

 DI w MauiProgram.cs:

ITouchInjector, IGamepadInputService, MobaInputMapper, MainPage, TestPage.

 Podstawowa obsługa gamepada na MainPage + TestPage.

Ten etap to stan „konfigurator działa, gamepad jest widziany przez aplikację”.

Etap 1 – Profile i konfiguracja

Cel: mieć sensowny system profili mapowania.

Zadania:

 Repozytorium profili (ProfilesRepository):

wczytywanie profili z wbudowanych danych / lokalnego pliku.

docelowo: zapis do Preferences / JSON w storage (persistencja między uruchomieniami).

 Na MainPage:

wybór profilu (Picker),

wyświetlanie aktywnego profilu,

przy zmianie profilu → aktualizacja MobaInputMapper.UpdateMappings.

 Na TestPage:

modyfikowanie Profile.Mappings (drag, size, type),

po powrocie do MainPage → aktualny profil ma zaktualizowane mapowania.

Etap 2 – Androidowy runtime dla gier (kluczowe dla „da się grać”)

Cel: sprawić, że mapowanie działa, kiedy na ekranie jest inna aplikacja (gra).

Zadania:

Dopiąć TouchAccessibilityService

 Plik accessibilityservice_config.xml w Resources/xml:

poprawne serviceInfo, description, eventTypes, feedbackType itd.

 Wpis w AndroidManifest.xml:

<service ... android:name=".TouchAccessibilityService" android:permission="android.permission.BIND_ACCESSIBILITY_SERVICE"> ....

 Upewnić się, że:

usługa jest widoczna w ustawieniach dostępności,

można ją włączyć i ma uprawnienia.

Połączenie GamepadInputService z usługą runtime

 Zaprojektować mechanizm, w którym:

GamepadInputService dostarcza GamepadState nie tylko do MAUI, ale też do „runtime controller”.

 Utworzyć np. klasę RuntimeInputController (w warstwie współdzielonej):

singleton, ma referencję do MobaInputMapper,

subskrybuje IGamepadInputService.GamepadUpdated,

wywołuje MobaInputMapper.OnGamepadStateChanged niezależnie od tego, czy MAUI UI jest na wierzchu.

Integracja z TouchAccessibilityService

 AndroidTouchInjector ma używać wyłącznie TouchAccessibilityService.Instance.

 MobaInputMapper wywołuje TapAsync / SwipeAsync, które w praktyce robią dispatchGesture na widoczną aplikację (grę).

Efekt: gdy użytkownik ma włączoną usługę dostępności i uruchomi grę, naciśnięcia na padzie będą tłumaczone na dotyk w tej grze.

Etap 3 – Obsługa lewego analoga (wirtualny joystick)

Cel: płynny ruch postaci w MOBA przy użyciu lewego drążka.

Koncepcja:

Konfiguracja:

w ActionMapping dodaj specjalny rodzaj akcji np. MoveJoystick,

mapowanie wskazuje na centrum wirtualnego joysticka w UI gry.

Logika:

gdy LeftStick wychyla się z (0,0):

jeśli nie ma aktywnego „touch drag”:

rozpocznij dotyk w centrum joysticka,

cyklicznie (np. co 16ms) aktualizuj pozycję dotyku proporcjonalnie do LeftStick.X/Y w promieniu R.

gdy LeftStick wraca do (0,0):

puść dotyk (lift finger).

Zadania:

 W GamepadState upewnić się, że LeftStick jest aktualizowany.

 W MobaInputMapper dodać obsługę nowego ActionType (np. MoveJoystick).

 W warstwie touch injection:

dodać wsparcie dla drag gesture trwającego w czasie (nie tylko jednorazowe swipe).

Etap 4 – MOBA–specyficzne akcje i profile

Cel: mieć co najmniej jeden gotowy, działający profil dla konkretnej gry MOBA.

Zadania:

 Dodać typy akcji:

QuickCast (tap w pozycję + od razu aktywacja),

NormalCast (aim & release – już prawie jest),

DoubleTap (np. dla szybkiego powrotu / recall),

LongPress (np. dla specjalnych umiejętności).

 Stworzyć profil np. „Wild Rift – przykładowy”:

manualnie pomierzyć tutaj współrzędne przycisków w grze,

wprowadzić gotowy zestaw ActionMapping.

 Dodać wizualne „preview” (screen gry jako tło na TestPage), żeby łatwiej ustawić mapowanie na właściwe miejsca.

Etap 5 – UX, bezpieczeństwo i wygoda

Cel: sprawić, że korzystanie z aplikacji jest komfortowe i bezpieczne.

Zadania:

 Ekran startowy z krótkim onboardingiem:

jak włączyć usługę dostępności,

jak wybrać profil i przełączyć się do gry.

 Przycisk „pauza mapowania”:

np. kombinacja przycisków na padzie, która wyłącza chwilowo emulację dotyku.

 Diagnostyka:

prosty ekran pokazujący ostatnie eventy gamepada i gesty wysyłane do dotyku.

 Eksport / import profili (np. JSON), żeby można było dzielić się konfiguracjami.

4. Definicja „Done” – kiedy uznajemy, że wymóg jest spełniony

Za spełnienie wymogu uznajemy stan, w którym:

Użytkownik:

uruchamia MOBaPadMapper2,

wybiera profil dla gry,

włącza usługę dostępności,

uruchamia grę MOBA (osobna aplikacja),

może swobodnie sterować postacią i umiejętnościami przy pomocy samego gamepada.

Aplikacja:

działa stabilnie w tle,

reaguje na przyciski i analogi gamepada,

wysyła tapy / swipy / drag do gry,

pozwala skonfigurować mapowanie bez grzebania w kodzie (tylko przez UI).
