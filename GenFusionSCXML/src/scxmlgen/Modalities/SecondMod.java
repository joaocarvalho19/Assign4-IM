package scxmlgen.Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum SecondMod implements IModality{
    
    
    GIVEUP_GES("[2][giveup]",2000),
    NEWGAME_GES("[3][newgamev]",1500),
    ACCEPT_GES("[0][acceptv]",1500),
    CLUE_GES("[1][cluev]",1500),

    THEME_GES("[5][themev]",3000);
    ;
    
    private String event;
    private int timeout;


    SecondMod(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
