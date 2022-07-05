/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package genfusionscxml;

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
import scxmlgen.Modalities.Output;
import scxmlgen.Modalities.Speech;
import scxmlgen.Modalities.SecondMod;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();
  

    
    fg.Single(SecondMod.GIVEUP_GES, Output.GIVEUP); 
    fg.Single(SecondMod.NEWGAME_GES, Output.NEWGAME); 
    fg.Single(SecondMod.ACCEPT_GES, Output.ACCEPT); 
    fg.Single(SecondMod.CLUE_GES, Output.CLUE);

    fg.Single(Speech.GIVEUP, Output.GIVEUP); 
    fg.Single(Speech.NEWGAME, Output.NEWGAME); 
    fg.Single(Speech.ACCEPT, Output.ACCEPT);
    fg.Single(Speech.CLUE, Output.CLUE);

    // Redundancy
    //fg.Redundancy(Speech.GIVEUP, SecondMod.GIVEUP_GES, Output.GIVEUP);
    
    // Complementarity
    fg.Complementary(Speech.BLUE, SecondMod.THEME_GES, Output.THEME_BLUE);
    fg.Complementary(SecondMod.THEME_GES, Speech.RED, Output.THEME_RED);
    fg.Complementary(SecondMod.THEME_GES, Speech.BROWN, Output.THEME_BROWN);
    fg.Complementary(SecondMod.THEME_GES, Speech.GREEN, Output.THEME_GREEN);
    fg.Complementary(SecondMod.THEME_GES, Speech.ORANGE, Output.THEME_ORANGE);
    fg.Build("fusion.scxml");
        
        
    }
    
}
