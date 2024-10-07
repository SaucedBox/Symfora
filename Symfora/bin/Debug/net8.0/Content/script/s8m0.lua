import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function trigger_trig_fallPlain1()
    ST.FallKillPlayer()
end

function trigger_trig_boss()
    ST.AwakeBoss("boss")
end

function init()
    ST.SetAllEnts("bossArena", false)
end

function custom_azaprota()
    ST.SetAllEnts("bossArena", true)
end