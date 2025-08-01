-create-3rd-person =
    { $chance ->
        [1] Создает
       *[other] создаёт
    }
-cause-3rd-person =
    { $chance ->
        [1] Вызывает
       *[other] вызывает
    }
-satiate-3rd-person =
    { $chance ->
        [1] Насыщает
       *[other] насыщает
    }
-create-3rd-person =
    { $chance ->
        [1] создает
       *[other] Создает
    }
-cause-3rd-person =
    { $chance ->
        [1] вызывает
       *[other] Вызывает
    }
-satiate-3rd-person =
    { $chance ->
        [1] насыщает
       *[other] Насыщает
    }
reagent-effect-guidebook-create-entity-reaction-effect =
    { $chance ->
        [1] создаёт
       *[other] Создаёт
    } { $amount ->
        [1] { INDEFINITE($entname) }
       *[other] { $amount } { MAKEPLURAL($entname) }
    }
reagent-effect-guidebook-explosion-reaction-effect =
    { $chance ->
        [1] вызывает
       *[other] Вызывает
    } взрыв
reagent-effect-guidebook-foam-area-reaction-effect =
    { $chance ->
        [1] создаёт
       *[other] Создаёт
    } большое количество пены
reagent-effect-guidebook-foam-area-reaction-effect =
    { $chance ->
        [1] создаёт
       *[other] Создаёт
    } большое количество дыма
reagent-effect-guidebook-satiate-thirst =
    { $chance ->
        [1] насыщает
       *[other] Насыщает
    } { $relative ->
        [1] жажду средне
       *[other] жажду на { NATURALFIXED($relative, 3) }x от среднего
    }
reagent-effect-guidebook-satiate-hunger =
    { $chance ->
        [1] насыщает
       *[other] Насыщает
    } { $relative ->
        [1] голод средне
       *[other] голод на { NATURALFIXED($relative, 3) }x от среднего
    }
reagent-effect-guidebook-health-change =
    { $chance ->
        [1]
            { $healsordeals ->
                [heals] Лечит
                [deals] Наносит
               *[both] Изменяет здоровье на
            }
       *[other]
            { $healsordeals ->
                [heals] лечит
                [deals] наносит
               *[both] Изменяет здоровье на
            }
    } { $changes }
reagent-effect-guidebook-status-effect =
    { $type ->
        [add]
            { $chance ->
                [1] Вызывает
               *[other] вызывает
            } { LOC($key) } как минимум на { NATURALFIXED($time, 3) } { $time } с накоплением
       *[set]
            { $chance ->
                [1] Вызывает
               *[other] вызывает
            } { LOC($key) } как минимум на { NATURALFIXED($time, 3) } { $time } без накоплением
        [remove]
            { $chance ->
                [1] Убирает
               *[other] убирает
            } { NATURALFIXED($time, 3) } { $time } { LOC($key) }
    }
reagent-effect-guidebook-activate-artifact =
    { $chance ->
        [1] Пытается
       *[other] пытается
    } активировать артефакт
reagent-effect-guidebook-set-solution-temperature-effect =
    { $chance ->
        [1] Устаналивает
       *[other] устанавливает
    } температуру раствора точно { NATURALFIXED($temperature, 2) }К
reagent-effect-guidebook-adjust-solution-temperature-effect =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Добавляет
               *[-1] Убирает
            }
       *[other]
            { $deltasign ->
                [1] добавляет
               *[-1] убирает
            }
    } тепло раствору до тех пор, пока он не достигнет { $deltasign ->
        [1] не больше { NATURALFIXED($maxtemp, 2) }К
       *[-1] не меньше { NATURALFIXED($mintemp, 2) }К
    }
reagent-effect-guidebook-adjust-reagent-reagent =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Adds
               *[-1] Removes
            }
       *[other]
            { $deltasign ->
                [1] add
               *[-1] remove
            }
    } { NATURALFIXED($amount, 2) }u of { $reagent } { $deltasign ->
        [1] to
       *[-1] from
    } the solution
reagent-effect-guidebook-adjust-reagent-group =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Adds
               *[-1] Removes
            }
       *[other]
            { $deltasign ->
                [1] add
               *[-1] remove
            }
    } { NATURALFIXED($amount, 2) }u of reagents in the group { $group } { $deltasign ->
        [1] to
       *[-1] from
    } the solution
reagent-effect-guidebook-adjust-temperature =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Adds
               *[-1] Removes
            }
       *[other]
            { $deltasign ->
                [1] add
               *[-1] remove
            }
    } { POWERJOULES($amount) } of heat { $deltasign ->
        [1] to
       *[-1] from
    } the body it's in
reagent-effect-guidebook-chem-cause-disease =
    { $chance ->
        [1] Causes
       *[other] cause
    } the disease { $disease }
reagent-effect-guidebook-chem-cause-random-disease =
    { $chance ->
        [1] Causes
       *[other] cause
    } the diseases { $diseases }
reagent-effect-guidebook-jittering =
    { $chance ->
        [1] Causes
       *[other] cause
    } jittering
reagent-effect-guidebook-chem-clean-bloodstream =
    { $chance ->
        [1] Cleanses
       *[other] cleanse
    } the bloodstream of other chemicals
reagent-effect-guidebook-cure-disease =
    { $chance ->
        [1] Cures
       *[other] cure
    } diseases
reagent-effect-guidebook-cure-eye-damage =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Heals
               *[-1] Deals
            }
       *[other]
            { $deltasign ->
                [1] heal
               *[-1] deal
            }
    } eye damage
reagent-effect-guidebook-chem-vomit =
    { $chance ->
        [1] Causes
       *[other] cause
    } vomiting
reagent-effect-guidebook-create-gas =
    { $chance ->
        [1] Creates
       *[other] create
    } { $moles } { $moles ->
        [1] mole
       *[other] moles
    } of { $gas }
reagent-effect-guidebook-drunk =
    { $chance ->
        [1] Causes
       *[other] cause
    } drunkness
reagent-effect-guidebook-electrocute =
    { $chance ->
        [1] Electrocutes
       *[other] electrocute
    } the metabolizer for { NATURALFIXED($time, 3) } { $time }
reagent-effect-guidebook-extinguish-reaction =
    { $chance ->
        [1] Extinguishes
       *[other] extinguish
    } fire
reagent-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Increases
       *[other] increase
    } flammability
reagent-effect-guidebook-ignite =
    { $chance ->
        [1] Ignites
       *[other] ignite
    } the metabolizer
reagent-effect-guidebook-make-sentient =
    { $chance ->
        [1] Makes
       *[other] make
    } the metabolizer sentient
reagent-effect-guidebook-modify-bleed-amount =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Induces
               *[-1] Reduces
            }
       *[other]
            { $deltasign ->
                [1] induce
               *[-1] reduce
            }
    } bleeding
reagent-effect-guidebook-modify-blood-level =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Increases
               *[-1] Decreases
            }
       *[other]
            { $deltasign ->
                [1] increases
               *[-1] decreases
            }
    } blood level
reagent-effect-guidebook-paralyze =
    { $chance ->
        [1] Paralyzes
       *[other] paralyze
    } the metabolizer for at least { NATURALFIXED($time, 3) } { $time }
reagent-effect-guidebook-cure-zombie-infection =
    { $chance ->
        [1] Cures
       *[other] cure
    } an ongoing zombie infection
reagent-effect-guidebook-cause-zombie-infection =
    { $chance ->
        [1] Gives
       *[other] give
    } an individual the zombie infection
reagent-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Cures
       *[other] cure
    } an ongoing zombie infection, and provides immunity to future infections
reagent-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Modifies
       *[other] modify
    } movement speed by { NATURALFIXED($walkspeed, 3) }x for at least { NATURALFIXED($time, 3) } { $time }
reagent-effect-guidebook-reset-narcolepsy =
    { $chance ->
        [1] Temporarily staves
       *[other] temporarily stave
    } off narcolepsy
reagent-effect-guidebook-wash-cream-pie-reaction =
    { $chance ->
        [1] Washes
       *[other] wash
    } off cream pie from one's face
reagent-effect-guidebook-missing =
    { $chance ->
        [1] Causes
       *[other] cause
    } an unknown effect as nobody has written this effect yet

# Amour
reagent-effect-guidebook-sex-change =
    { $chance ->
    [1] Делает
    *[other] делает
        } метаболизатор меняет пол
reagent-effect-guidebook-gender-change =
    { $chance ->
    [1] Делает
    *[other] делает
        } метаболизатор меняет пол
