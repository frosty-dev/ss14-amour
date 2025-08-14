game-ticker-restart-round = Залупа раунда...
game-ticker-start-round = Залупа начинается...
game-ticker-start-round-cannot-start-game-mode-fallback = Не удалось запустить режим { $failedGameMode }! Запускаем { $fallbackMode }...
game-ticker-start-round-cannot-start-game-mode-restart = Не удалось запустить режим { $failedGameMode }! Перезапуск раунда...
game-ticker-unknown-role = Неизвестный
game-ticker-delay-start = Начало раунда было отложено на { $seconds } секунд.
game-ticker-pause-start = Начало раунда было приостановлено.
game-ticker-pause-start-resumed = Отсчет начала раунда возобновлен.
game-ticker-player-join-game-message = Салам Алейкум БРАТ. На этом сервере тебе дадут 100 обещаний и ни одно не выполнят. ТОЛЬКО НА НЕМ ты можешь узреть хоста трапа и его подсоса с именем фирмы телефонов.
game-ticker-get-info-text =
    Самсунг Гелакси С выебан раз: [color=white]#{ $roundId }[/color]
    Количество обещаний трапа хоста: [color=white]{ $playerCount }[/color]
    На этой карте выебан самсунг: [color=white]{ $mapName }[/color]
    Обещанный режим игры: [color=white]{ $gmTitle }[/color]

game-ticker-get-info-preround-text =
    Самсунг Гелакси С выебан раз: [color=white]#{ $roundId }[/color]
    Количество обещаний трапа хоста: [color=white]{ $playerCount }[/color] ([color=white]{ $readyCount }[/color] { $readyCount ->
        [one] готов
       *[other] готовы
    })
    На этой карте выебан самсунг: [color=white]{ $mapName }[/color]
    Обещанный режим игры: [color=white]{ $gmTitle }[/color]

game-ticker-no-map-selected = [color=red]Место ебания самсунга еще не выбрано![/color]
game-ticker-player-no-jobs-available-when-joining = короче нельзя зайти иди нахуй.
# Displayed in chat to admins when a player joins
player-join-message = Игрок { $name } присоединился к серверу!
# Displayed in chat to admins when a player leaves
player-leave-message = Игрок { $name } покинул сервер!
latejoin-arrival-announcement =
    { $character } ({ $job }) { $gender ->
        [male] прибыл
        [female] прибыла
        [epicene] прибыли
       *[neuter] прибыл
    } на станцию!
latejoin-arrival-sender = Станции
player-join-message = Игрок { $name } зашёл!
latejoin-arrivals-direction = Вскоре прибудет шаттл, который доставит вас на вашу станцию.
player-first-join-message = Игрок { $name } зашёл на сервер впервые.
player-leave-message = Игрок { $name } вышел!
