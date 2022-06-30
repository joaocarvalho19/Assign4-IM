using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;


namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;

        //  new 16 april 2020
        private MmiCommunication mmiSender;
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        Boolean hint_flag = false;
        Boolean newgame_flag = false;
        int current_hints_num = 0;

        String chess_url = "https://www.chess.com/play/computer";

        IWebDriver driver;

        // Keep the coordinates of pieces
        Dictionary<string, (string, int)> coordinates =
                       new Dictionary<string, (string, int)>();

        // Last move
        string[] last_move = { "", "", "" };

        public void get_coordinates()
        {
            coordinates.Add("white_rook_1", ("square-11", 0));
            coordinates.Add("white_rook_2", ("square-81", 0));
            coordinates.Add("white_knight_1", ("square-21", 0));
            coordinates.Add("white_knight_2", ("square-71", 0));
            coordinates.Add("white_bishop_1", ("square-31", 0));
            coordinates.Add("white_bishop_2", ("square-61", 0));
            coordinates.Add("white_queen", ("square-41", 0));
            coordinates.Add("white_king", ("square-51", 0));

            for (int i = 1; i<=8; i++)
            {
                coordinates.Add("white_pawn_"+i, ("square-"+i+"2", 0));
            }
            coordinates.Add("black_rook_1", ("square-18", 0));
            coordinates.Add("black_rook_2", ("square-88", 0));
            coordinates.Add("black_knight_1", ("square-28", 0));
            coordinates.Add("black_knight_2", ("square-78", 0));
            coordinates.Add("black_bishop_1", ("square-38", 0));
            coordinates.Add("black_bishop_2", ("square-68", 0));
            coordinates.Add("black_queen", ("square-48", 0));
            coordinates.Add("black_king", ("square-58", 0));

            for (int i = 1; i <= 8; i++)
            {
                coordinates.Add("black_pawn_" + i, ("square-" + i + "7", 0));
        }
        }
         // Move a Piece
        public void move(string piece, string _to)
        {
            try
            {
                Actions action = new Actions(driver);

                Console.WriteLine("COORDINATES BEFORE: " + piece + " - " + coordinates[piece]);
                string class_coord = coordinates[piece].Item1;
                IWebElement piece_from = driver.FindElement(By.ClassName(class_coord));
                
                piece_from.Click();

                System.Threading.Thread.Sleep(1000);

                IWebElement piece_to = driver.FindElement(By.ClassName(_to));
                action.ClickAndHold(piece_from).MoveToElement(piece_to).Release().Build().Perform();

                // update coordenates
                int current_num_moves = coordinates[piece].Item2;
                coordinates[piece] = (_to, current_num_moves+1);

                //Last move
                last_move[0] = piece;    //piece
                last_move[1] = class_coord;    //old position
                last_move[2] = _to;    //new position
                Console.WriteLine("COORDINATES AFTER: "+piece+" - "+coordinates[piece]);
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR moving a piece {0}", e);
            }

        }

        public int get_num_moves(string piece)
            {return coordinates[piece].Item2; }

        public void get_target_class(string piece, string _to)
        {
            try
            {
                Actions action = new Actions(driver);
                string class_coord = coordinates[piece].Item1;
                IWebElement piece_from = driver.FindElement(By.ClassName(class_coord));
                piece_from.Click();

                System.Threading.Thread.Sleep(1000);

                IWebElement piece_to = driver.FindElement(By.ClassName(_to));
                action.ClickAndHold(piece_from).MoveToElement(piece_to).Release().Build().Perform();
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR moving a piece {0}", e);
            }

        }

        // Abort the game
        public void abort_game()
        {
            try { 
                IList<IWebElement> controlsButtoms = driver.FindElements(By.ClassName("primary-controls-button"));
                IWebElement new_gameButtom = controlsButtoms[0];
                new_gameButtom.Click();

                IWebElement yesButton = driver.FindElement(By.XPath("//button[.='Yes']"));
                yesButton.Click();

                coordinates.Clear();
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }
    
        }
        public void start_new_game()
        {
            try
            {
                IList<IWebElement> controlsButtoms = driver.FindElements(By.ClassName("game-over-controls-buttonlg"));
                IWebElement new_gameButtom = controlsButtoms[1];
                new_gameButtom.Click();

                get_coordinates();
                System.Threading.Thread.Sleep(1000);
                IWebElement ChooseButton = driver.FindElement(By.XPath("//button[.='Choose']"));
                ChooseButton.Click();
                System.Threading.Thread.Sleep(1000);
                IWebElement PlayButton = driver.FindElement(By.XPath("//button[.='Play']"));
                PlayButton.Click();
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR Starting new game {0}", e);
            }

        }

        public void undo_move()
        {
            try
            {   
                IList<IWebElement> controlsButtoms = driver.FindElements(By.ClassName("primary-controls-button"));
                IWebElement undoButtom = controlsButtoms[1];
                undoButtom.Click();

                //Last move
                string piece = last_move[0];    //piece
                string old_pos = last_move[1];    //old position
                string new_pos = last_move[2];    //new position
                
                // update coordenates
                int current_num_moves = coordinates[piece].Item2;
                coordinates[piece] = (old_pos, current_num_moves - 1);

                Console.WriteLine(coordinates[piece]);
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR undo a move {0}", e);
            }

        }

        public void redo_move()
        {
            try
            {
                IList<IWebElement> controlsButtoms = driver.FindElements(By.ClassName("primary-controls-button"));
                IWebElement undoButtom = controlsButtoms[2];
                undoButtom.Click();

                //Last move
                string piece = last_move[0];    //piece
                string old_pos = last_move[1];    //old position
                string new_pos = last_move[2];    //new position

                // update coordenates
                int current_num_moves = coordinates[piece].Item2;
                coordinates[piece] = (new_pos, current_num_moves + 1);

                Console.WriteLine(coordinates[piece]);
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR redo a move {0}", e);
            }

        }

        public void show_clue()
        {
            try
            {
                IList<IWebElement> controlsButtoms = driver.FindElements(By.ClassName("primary-controls-button"));
                IWebElement undoButtom = controlsButtoms[3];
                undoButtom.Click();
                System.Threading.Thread.Sleep(3000);
                if (current_hints_num == 3)
                {
                    Console.WriteLine("CLICK GET HINT");
                    //click_get_hint();
                }
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR show a clue {0}", e);
            }

        }

        // Simple click on the page
        public void click(string _class)
        {
            IWebElement buttom = driver.FindElement(By.ClassName(_class));
            buttom.Click();
        }

        // Open settings
        public void open_settings()
        {
            try { 
                click("circle-gearwheel");
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }
        }

        // Change the color of the board
        public void change_color()
        {
            try {
                // Get all elements with a given ClassName

                IList<IWebElement> all = driver.FindElements(By.ClassName("settings-select"));

                //Get a random index
                Random r = new Random();
                int index = r.Next(1, 29); //for ints
                //create select element object 
                var selectElement = new SelectElement(all[1]);

                //select by value
                selectElement.SelectByIndex(index);

            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }

        }

        public void change_pieces()
        {
            try
            {
                // Get all elements with a given ClassName

                IList<IWebElement> all = driver.FindElements(By.ClassName("settings-select"));

                //Get a random index
                Random r = new Random();
                int index = r.Next(1, 38); //for ints
                //create select element object 
                var selectElement = new SelectElement(all[0]);

                //select by value
                selectElement.SelectByIndex(index);

            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }

        }
        public void save_options()
        {
            try
            {
                //Save
                IList<IWebElement> settingsButtons = driver.FindElements(By.ClassName("settings-modal-container-button"));

                IWebElement saveButtom = settingsButtons[1];
                saveButtom.Click();

            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR saving {0}", e);
            }
        }

        public void cancel_options()
        {
            // "circle-gearwheel" -- settings

            try
            {
                //Cancel
                IList<IWebElement> settingsButtons = driver.FindElements(By.ClassName("settings-modal-container-button"));

                IWebElement cancelButtom = settingsButtons[0];
                cancelButtom.Click();

            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR cancel {0}", e);
            }
        }

        public void sounds()
        {
            // "circle-gearwheel" -- settings
            
            try
            {
                IList<IWebElement> all = driver.FindElements(By.ClassName("ui_v5-switch-button"));

                // Get all elements with a given ClassName
                IWebElement soundsButton = all[1];
                soundsButton.Click();

                
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR {0}", e);
            }

        }

        public void scroll_options()
        {

            try
            {
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                System.Threading.Thread.Sleep(500);
                IWebElement Element = driver.FindElement(By.XPath("//*[@id='board-layout-chessboard']/div[4]/div[2]/div[1]/div/div[12]/div[1]"));
                js.ExecuteScript("arguments[0].scrollIntoView();", Element);
                Console.Read();
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }

        }

        public void testpawn(string piece, int quantity, string ordinal) {

            if (piece != null && quantity != 0 && ordinal != null)// ex: FIRST PAWN TWO   - PEAO
            {
                string final_piece = piece + ordinal;
                string piece_square = coordinates[final_piece].Item1;
                int row = piece_square[piece_square.Length - 1] - '0';
                int final_row = row + quantity;

                string target_sqare = piece_square.Remove(piece_square.Length - 1, 1) + final_row;
                Console.WriteLine("FINAL PIECE: " + target_sqare);
                move(final_piece, target_sqare);
            }
        }

        public string get_piece_name(string cood) {
            string piece_name = null;
            foreach (KeyValuePair<string, (string, int)> entry in coordinates)
            {
                if (entry.Value.Item1 == cood) {
                    piece_name = entry.Key;
                }
            }
            return piece_name;
        }
        
        public void hint_move() {
            try
            {
                IList<IWebElement> candidates = driver.FindElements(By.ClassName("highlight"));

                IList<IWebElement> _from_coord = null;
                IList<IWebElement> _to_coord = null;
                string coord_to_name = null;

                string correct_piece = null;

                for (int i = 0; i < candidates.Count; i++)
                {
                    String buttonColor = candidates[i].GetCssValue("background-color");
                    if (buttonColor.Contains("204")) {
                        string coord = candidates[i].GetAttribute("class").Split(' ')[1];
                        string piece = get_piece_name(coord);
                        if (piece != null && piece.Contains("white")) {
                            _from_coord = driver.FindElements(By.ClassName(coord));
                            Console.WriteLine(" COUNT COORD FROM - {0}", _from_coord.Count);
                            correct_piece = piece;
                            Console.WriteLine(" PIECE - {0}", piece);
                            
                        }
                        else { 
                            _to_coord = driver.FindElements(By.ClassName(coord));

                            coord_to_name = coord;
                            Console.WriteLine(" COORD final - {0}", _to_coord.Count);
                        }
                        Console.WriteLine(" Color - {0}", buttonColor);
                    }
                }

                Actions action = new Actions(driver);

                _from_coord[_from_coord.Count - 1].Click();
                System.Threading.Thread.Sleep(1000);

                action.ClickAndHold(_from_coord[_from_coord.Count - 1]).MoveToElement(_to_coord[_to_coord.Count-1]).Release().Build().Perform();

                // update coordenates
                int current_num_moves = coordinates[correct_piece].Item2;
                coordinates[correct_piece] = (coord_to_name, current_num_moves + 1);

                Console.WriteLine(" HINT MOVE - {0} - {1}", correct_piece, coordinates[correct_piece]);
                current_hints_num++;
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }
        }
        public void click_get_hint() {
            IWebElement ghButton = driver.FindElement(By.XPath("//button[.='Get Hint']"));
            ghButton.Click();
        }


        // Start de browser
        [SetUp]
        public void start_Browser()
        {
            try {
                // Local Selenium WebDriver
                driver = new ChromeDriver();
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(chess_url);

                IWebElement okButton = driver.FindElement(By.XPath("//button[.='Ok']"));
                okButton.Click();
            }
            catch (WebDriverException e)
            {
                Console.WriteLine("ERROR {0}", e);
            }
        }


        public MainWindow()
        {
            InitializeComponent();


            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

            //init LifeCycleEvents..
            lce = new LifeCycleEvents("APP", "TTS", "User1", "na", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode
            // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GUI");


            // CHESS APP -----------------------------------------------------------
            get_coordinates();
            foreach (KeyValuePair<string, (string, int)> kvp in coordinates)
            {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }
            
            start_Browser();
            move("white_pawn_5", "square-54");

            /*System.Threading.Thread.Sleep(7000);
            show_clue();
            System.Threading.Thread.Sleep(3000);
            hint_move();
            System.Threading.Thread.Sleep(3000);
            show_clue();
            System.Threading.Thread.Sleep(3000);
            hint_move();
            System.Threading.Thread.Sleep(3000);
            show_clue();
            System.Threading.Thread.Sleep(3000);
            hint_move();
            System.Threading.Thread.Sleep(3000);
            show_clue();
            System.Threading.Thread.Sleep(3000);
            hint_move();
            System.Threading.Thread.Sleep(3000);
            show_clue();
            System.Threading.Thread.Sleep(3000);
            hint_move();
            System.Threading.Thread.Sleep(3000);
            show_clue();
            System.Threading.Thread.Sleep(3000);
            hint_move();
            show_clue();
            System.Threading.Thread.Sleep(5000);
            
            System.Threading.Thread.Sleep(5000);

            System.Threading.Thread.Sleep(5000);

            //one_direction("white_rook_", 2, "1", "up");
            two_directions("white_queen", 3, "right", "up");
            System.Threading.Thread.Sleep(5000);

            testpawn("white_pawn_", 2, "1");

            System.Threading.Thread.Sleep(5000);

            two_directions("white_queen", 2, "left", "down");

            */
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine("-------RECEIVED!!!!");
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            // Print income msg
            Console.WriteLine("INCOME MSG: "+(string)json.recognized[0].ToString());
            string piece = null;
            string ordinal = null;
            int quantity = 0;
            string coord = null;
            string direction = null;
            string direction2 = null;
            

            //switch (obj)
            string msg = json.recognized[0].ToString();
            List<string> elements = new List<string>(
                msg.Split(new string[] { " " }, StringSplitOptions.None));

            // Mexer Peão
            
            switch (msg)
            {
                /*case "acceptv":
                    if (hint_flag) { 
                        hint_move();
                        hint_flag = false;
                    }
                    else
                    {
                        Console.WriteLine("Hint false");
                    }
                    break;
                case "giveup":
                    newgame_flag = true;
                    abort_game();
                    break;
                case "newgame":
                    if (newgame_flag)
                    {
                        start_new_game();
                        newgame_flag = false;
                    }
                    else {
                        Console.WriteLine("New game false");
                    }
                    break;
                case "undo":
                    undo_move();
                    break;
                case "redo":
                    redo_move();
                    break;
                case "record":
                    hint_flag = true;
                    show_clue();
                    break;*/


                    case "YES_NEWGAME":
                        start_new_game();
                        break;
                    case "YES_CLUE":
                        hint_move();
                        break;
                    case "GIVEUP":
                        abort_game();
                        break;
                    case "NEWGAME":
                        start_new_game();
                        break;
                    case "OPTIONS":
                        open_settings();
                        break;
                    case "UNDO":
                        undo_move();
                        break;
                    case "REDO":
                        redo_move();
                        break;
                    case "CLUE":
                        show_clue();
                        break;
                    case "BOARD_COLOR":
                        change_color();
                        break;
                    case "PIECE_APPEARANCE":
                        change_pieces();
                        break;
                    case "SAVE":
                        save_options();
                        break;
                    case "CANCEL":
                        cancel_options();
                        break;
                    case "SCROLLDOWN":
                        scroll_options();
                        break;
                    case "NOSOUND":
                        sounds();
                        break;
                    case "SOUND":
                        sounds();
                        break;

                    case "QUEEN":
                        piece = "white_queen";
                        break;
                    case "KING":
                        piece = "white_king";
                        break;

                    case "FIRST":
                        ordinal = "1";
                        break;
                    case "SECOND":
                        ordinal = "2";
                        break;
                    case "THIRD":
                        ordinal = "3";
                        break;
                    case "FOURTH":
                        ordinal = "4";
                        break;
                    case "FIFTH":
                        ordinal = "5";
                        break;
                    case "SIXTH":
                        ordinal = "6";
                        break;
                    case "SEVENTH":
                        ordinal = "7";
                        break;
                    case "EIGHTH":
                        ordinal = "8";
                        break;

            }

            try
                {
                switch (elements[1])
                    {
                    case "PAWN":
                        piece = "white_pawn_";
                        break;
                    case "ROOK":
                        piece = "white_rook_";
                        break;
                    case "BISHOP":
                        piece = "white_bishop_";
                        break;
                    case "KNIGHT":
                        piece = "white_knight_";
                        break;

                    case "ONE":
                        quantity = 1;
                        break;
                    case "TWO":
                        quantity = 2;
                        break;
                    case "THREE":
                        quantity = 3;
                        break;
                    case "FOUR":
                        quantity = 4;
                        break;
                    case "FIVE":
                        quantity = 5;
                        break;
                    case "SIX":
                        quantity = 6;
                        break;
                    case "SEVEN":
                        quantity = 7;
                        break;
                    case "EIGTH":
                        quantity = 8;
                        break;
                }
            }
            catch (Exception ex)
                {
                    Console.WriteLine("ERROR {0}", ex);
                }
            try
            {
                switch (elements[2])
                {
                    case "A1":coord = "square-11";
                        break;
                    case "A2":
                        coord = "square-12";
                        break;
                    case "A3":
                        coord = "square-13";
                        break;
                    case "A4":
                        coord = "square-14";
                        break;
                    case "A5":
                        coord = "square-15";
                        break;
                    case "A6":
                        coord = "square-16";
                        break;
                    case "A7":
                        coord = "square-17";
                        break;
                    case "A8":
                        coord = "square-18";
                        break;

                    case "B1":
                        coord = "square-21";
                        break;
                    case "B2":
                        coord = "square-22";
                        break;
                    case "B3":
                        coord = "square-23";
                        break;
                    case "B4":
                        coord = "square-24";
                        break;
                    case "B5":
                        coord = "square-25";
                        break;
                    case "B6":
                        coord = "square-26";
                        break;
                    case "B7":
                        coord = "square-27";
                        break;
                    case "B8":
                        coord = "square-28";
                        break;

                    case "C1":
                        coord = "square-31";
                        break;
                    case "C2":
                        coord = "square-32";
                        break;
                    case "C3":
                        coord = "square-33";
                        break;
                    case "C4":
                        coord = "square-34";
                        break;
                    case "C5":
                        coord = "square-35";
                        break;
                    case "C6":
                        coord = "square-36";
                        break;
                    case "C7":
                        coord = "square-37";
                        break;
                    case "C8":
                        coord = "square-38";
                        break;

                    case "D1":
                        coord = "square-41";
                        break;
                    case "D2":
                        coord = "square-42";
                        break;
                    case "D3":
                        coord = "square-43";
                        break;
                    case "D4":
                        coord = "square-44";
                        break;
                    case "D5":
                        coord = "square-45";
                        break;
                    case "D6":
                        coord = "square-46";
                        break;
                    case "D7":
                        coord = "square-47";
                        break;
                    case "D8":
                        coord = "square-48";
                        break;

                    case "E1":
                        coord = "square-51";
                        break;
                    case "E2":
                        coord = "square-52";
                        break;
                    case "E3":
                        coord = "square-53";
                        break;
                    case "E4":
                        coord = "square-54";
                        break;
                    case "E5":
                        coord = "square-55";
                        break;
                    case "E6":
                        coord = "square-56";
                        break;
                    case "E7":
                        coord = "square-57";
                        break;
                    case "E8":
                        coord = "square-58";
                        break;

                    case "F1":
                        coord = "square-61";
                        break;
                    case "F2":
                        coord = "square-62";
                        break;
                    case "F3":
                        coord = "square-63";
                        break;
                    case "F4":
                        coord = "square-64";
                        break;
                    case "F5":
                        coord = "square-65";
                        break;
                    case "F6":
                        coord = "square-66";
                        break;
                    case "F7":
                        coord = "square-67";
                        break;
                    case "F8":
                        coord = "square-68";
                        break;

                    case "G1":
                        coord = "square-71";
                        break;
                    case "G2":
                        coord = "square-72";
                        break;
                    case "G3":
                        coord = "square-73";
                        break;
                    case "G4":
                        coord = "square-74";
                        break;
                    case "G5":
                        coord = "square-75";
                        break;
                    case "G6":
                        coord = "square-76";
                        break;
                    case "G7":
                        coord = "square-77";
                        break;
                    case "G8":
                        coord = "square-78";
                        break;

                    case "H1":
                        coord = "square-81";
                        break;
                    case "H2":
                        coord = "square-82";
                        break;
                    case "H3":
                        coord = "square-83";
                        break;
                    case "H4":
                        coord = "square-84";
                        break;
                    case "H5":
                        coord = "square-85";
                        break;
                    case "H6":
                        coord = "square-86";
                        break;
                    case "H7":
                        coord = "square-87";
                        break;
                    case "H8":
                        coord = "square-88";
                        break;

                    case "ONE":
                        quantity = 1;
                        break;
                    case "TWO":
                        quantity = 2;
                        break;
                    case "THREE":
                        quantity = 3;
                        break;
                    case "FOUR":
                        quantity = 4;
                        break;
                    case "FIVE":
                        quantity = 5;
                        break;
                    case "SIX":
                        quantity = 6;
                        break;
                    case "SEVEN":
                        quantity = 7;
                        break;
                    case "EIGTH":
                        quantity = 8;
                        break;

                    case "UP":
                        direction = "up";
                        break;
                    case "DOWN":
                        direction = "down";
                        break;
                    case "LEFT":
                        direction = "left";
                        break;
                    case "RIGHT":
                        direction = "right";
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR {0}", ex);
            }
            try
            {
                switch (elements[3])
                {
                    case "UP":
                        if(direction == null) {direction = "up"; }
                        else { direction2 = "up"; }
                        break;
                    case "DOWN":
                        if (direction == null) { direction = "down"; }
                        else { direction2 = "down"; }
                        break;
                    case "LEFT":
                        if (direction == null) { direction = "left"; }
                        else { direction2 = "left"; }
                        break;
                    case "RIGHT":
                        if (direction == null) { direction = "rigth"; }
                        else { direction2 = "rigth"; }
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR {0}", ex);
            }
            try
            {
                switch (elements[4])
                {
                    case "UP":
                        direction2 = "up";
                        break;
                    case "DOWN":
                        direction2 = "down";
                        break;
                    case "LEFT":
                        direction2 = "left";
                        break;
                    case "RIGHT":
                        direction2 = "right";
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR {0}", ex);
            }

            /* - - - - - -  - Process - - - - - - - -*/

            // IF is a move with coordinates - Ex: d4
            if (piece != null && coord != null)
            {
                string final_piece = piece + ordinal;
                Console.WriteLine("FINAL PIECE: " + final_piece);
                move(final_piece, coord);
            }
            else {  // IF NOT
                // 0 diretions implies PAWN
                if (piece != null && quantity != 0 && ordinal != null && direction == null)// ex: FIRST PAWN TWO   - PEAO
                {
                    string final_piece = piece + ordinal;
                    string piece_square = coordinates[final_piece].Item1;
                    int row = piece_square[piece_square.Length - 1] - '0';
                    int final_row = row + quantity;

                    string target_sqare = piece_square.Remove(piece_square.Length - 1, 1) + final_row;
                    Console.WriteLine("FINAL PIECE: " + target_sqare);
                    move(final_piece, target_sqare);
                }


                // Only 1 direction - (up, down...)
                else if (piece != null && quantity != 0 && direction != null && direction2 == null) {
                    string final_piece = null;
                    if (ordinal == null) { final_piece = piece; }
                    else { final_piece = piece + ordinal;}

                    one_direction(final_piece, quantity, direction);
                }

                // 2 direction - (up left, down right ...)
                else if (piece != null && quantity != 0 && direction != null && direction2 != null)
                {
                    string final_piece = null;
                    if (ordinal == null) { final_piece = piece; }
                    else { final_piece = piece + ordinal; }

                    two_directions(final_piece, quantity, direction, direction2);
                }

            }


            mmic.Send(lce.NewContextRequest());

            string json2 = ""; // "{ \"synthesize\": [";

            json2 += msg;
            Console.WriteLine("HSON2: " + json2);
            //json2 += (string)json.recognized[1].ToString() + " DONE." ;
            //json2 += "] }";
            /*
             foreach (var resultSemantic in e.Result.Semantics)
            {
                json += "\"" + resultSemantic.Value.Value + "\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            */
            var exNot = lce.ExtensionNotification(0 + "", 0 + "", 1, json2);
            mmic.Send(exNot);


        }

        public void one_direction(string final_piece, int quantity, string direction)
        {
            // EX: FIRST ROOK TWO UP
            Console.WriteLine("ENTROU "+ final_piece + quantity+direction);
            string piece_square = coordinates[final_piece].Item1;
            int row, col, final_row, final_col;
            string target_square = null;

            if (direction == "up")
            {
                row = piece_square[piece_square.Length - 1] - '0';
                final_row = row + quantity;
                target_square = piece_square.Remove(piece_square.Length - 1, 1) + final_row;
            }
            else if (direction == "down")
            {
                row = piece_square[piece_square.Length - 1] - '0';
                final_row = row - quantity;
                target_square = piece_square.Remove(piece_square.Length - 1, 1) + final_row;
            }
            else if (direction == "left")
            {
                col = piece_square[piece_square.Length - 2] - '0';
                final_col = col - quantity;
                char last_char = piece_square[piece_square.Length - 1];
                target_square = piece_square.Remove(piece_square.Length - 2, 2) + final_col + last_char;
            }
            else // right
            {
                Console.WriteLine("ENTROU");
                col = piece_square[piece_square.Length - 2] - '0';
                final_col = col + quantity;
                char last_char = piece_square[piece_square.Length - 1];
                target_square = piece_square.Remove(piece_square.Length - 2, 2) + final_col + last_char;
            }


            Console.WriteLine("FINAL PIECE: " + target_square);
            move(final_piece, target_square);
        }

        public void two_directions(string final_piece, int quantity, string direction, string direction_2)
        {
            // EX: FIRST BISHOP TWO UP RIGHT
            string piece_square = coordinates[final_piece].Item1;
            int row, col, final_row, final_col;
            string target_square = null;

            if ((direction == "up" && direction_2 == "left") || (direction == "left" && direction_2 == "up"))
            {
                col = piece_square[piece_square.Length - 2] - '0';
                row = piece_square[piece_square.Length - 1] - '0';

                final_col = col - quantity;

                row = piece_square[piece_square.Length - 1] - '0';
                final_row = row + quantity;
                target_square = piece_square.Remove(piece_square.Length - 2, 2) + final_col + final_row;
            }

            else if ((direction == "up" && direction_2 == "right") || (direction == "right" && direction_2 == "up"))
            {
                col = piece_square[piece_square.Length - 2] - '0';
                row = piece_square[piece_square.Length - 1] - '0';

                final_col = col + quantity;

                row = piece_square[piece_square.Length - 1] - '0';
                final_row = row + quantity;
                target_square = piece_square.Remove(piece_square.Length - 2, 2) + final_col + final_row;
            }

            else if ((direction == "down" && direction_2 == "right") || (direction == "right" && direction_2 == "down"))
            {
                col = piece_square[piece_square.Length - 2] - '0';
                row = piece_square[piece_square.Length - 1] - '0';

                final_col = col + quantity;

                row = piece_square[piece_square.Length - 1] - '0';
                final_row = row - quantity;
                target_square = piece_square.Remove(piece_square.Length - 2, 2) + final_col + final_row;
            }

            else
            {
                col = piece_square[piece_square.Length - 2] - '0';
                row = piece_square[piece_square.Length - 1] - '0';

                final_col = col - quantity;

                row = piece_square[piece_square.Length - 1] - '0';
                final_row = row - quantity;
                target_square = piece_square.Remove(piece_square.Length - 2, 2) + final_col + final_row;
            }


            Console.WriteLine("FINAL PIECE: " + target_square);
            move(final_piece, target_square);
            
        }
    }
}
