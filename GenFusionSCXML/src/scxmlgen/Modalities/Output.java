package scxmlgen.Modalities;

import scxmlgen.interfaces.IOutput;



public enum Output implements IOutput{
    
    GIVEUP("[GIVEUP]"),
    GIVEUP_REDU("[GIVEUP_REDU]"),

    NEWGAME("[NEWGAME]"),
    CLUE("[CLUE]"),
    ACCEPT("[ACCEPT]"),
    
    THEME_BLUE("[THEME_BLUE]"),
    THEME_RED("[THEME_RED]"),
    THEME_BROWN("[THEME_BROWN]"),
    THEME_GREEN("[THEME_GREEN]"),
    THEME_ORANGE("[THEME_ORANGE]")
    ;
    
    
    
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
