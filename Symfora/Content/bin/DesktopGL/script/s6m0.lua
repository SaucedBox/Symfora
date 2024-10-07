import ('TripleS', 'TripleS.Scripting')
import ('Symfora', 'Symfora')

function keyInput_inp1()
    ST.FlipEnt("door2")
end
function keyInput_inp2()
    ST.FlipEnt("door6")
end
function keyInput_inp3()
    ST.FlipEnt("door9")
end

function switch_switch1(state)
    if state then
        oSwc = ST.GetEnt("switch2")
        if oSwc.Switched then
            ST.SetEnt("door1", false)
        end
    end
end
function switch_switch2(state)
    if state then
        oSwc = ST.GetEnt("switch1")
        if oSwc.Switched then
            ST.SetEnt("door1", false)
        end
        oSwc = ST.GetEnt("door3")
        if oSwc.Active then
            ST.SetEnt("door3", false)
            ST.SetEnt("timer1", false)
        end
    end
end

function timer_timer1(state)
    ST.FlipEnt("door3")
end
function timer_timer2(state)
    ST.FlipEnt("door4")
end

function switch_bal1_switch(state)
    if state then
        ST.ShootBallista("ballista1")
    end
end
function switch_bal2_switch(state)
    if state then
        ST.ShootBallista("ballista2")
    end
end
function switch_bal3_switch(state)
    if state then
        ST.ShootBallista("ballista3")
        col = ST.GetEnt("chloraClip")
        if col.Active then
            ST.SetAllEnts("chloraClip", false)
        end
    end
end

function magnet_magnet1(holding)
    ST.FlipEnt("door5")
end
function magnet_magnet2(holding)
    ST.FlipEnt("door7")
end
function magnet_magnet3(holding)
    ST.FlipEnt("door8")
end
function magnet_magnet4(holding)
    ST.FlipEnt("door11")
end

function switch_switch3(state)
    ST.FlipEnt("door10")
end