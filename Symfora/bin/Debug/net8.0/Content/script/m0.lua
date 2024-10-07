import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function init()
end

function switch_testSwitch(active)
    ST.FlipEnt("testDoor")
    ST.ShootBallista("testBall")
end

function timer_testTimer(state)
    ST.FlipEnt("testDoor")
end

function trigger_testTrig()
    ST.PlayMusic("testMusic", 1)
    ST.Tutorial(0)
    ST.Tutorial(1)
    ST.Tutorial(2)
    ST.Tutorial(3)
    ST.Tutorial(4)
    ST.Tutorial(5)
end