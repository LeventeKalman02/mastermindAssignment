using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft;
using Newtonsoft.Json;

namespace mastermindAssignment;

public partial class MainPage : ContentPage
{
    GameSave save = new GameSave();
    Random rand = new Random();
    Color[] colors = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.HotPink, Colors.Cyan };

    //declaration of variables
    int row = 0;
    int[] guess = { -1, -1, -1, -1 };
    int[] solution = new int[4];
    string path = AppDomain.CurrentDomain.BaseDirectory + "game_save.txt";

    public MainPage()
	{
        if (!File.Exists(path))
        {
            // Create a file to write to.
            File.CreateText(path);
        }

        GenerateSolution();
		InitializeComponent();
    }

    // generates the random colors that you have to guess
    private void GenerateSolution()
    {
        // stores the index of the random colors in the solution array
        solution[0] = rand.Next(0, colors.Length);
        solution[1] = rand.Next(0, colors.Length);
        solution[2] = rand.Next(0, colors.Length);
        solution[3] = rand.Next(0, colors.Length);
    }

    //saves the game to a file
    private void SaveGame(object sender, EventArgs e)
    {
        save.row = row;
        save.solution = solution;
        //if you have won the game (row > 10) then save the solution in the last row
        save.guesses[row > 10 ? 9 : row] = row > 10 ? solution : guess;

        using (StreamWriter sw = new StreamWriter(File.OpenWrite(path)))
        {
            sw.WriteLine(JsonConvert.SerializeObject(save));
        }
    }

    //loads the game save
    private void LoadGame(object sender, EventArgs e)
    {
        ResetBoard(null, null);
        using (StreamReader sr = File.OpenText(path))
        {
            save = JsonConvert.DeserializeObject<GameSave>(sr.ReadLine());
            //save contains the row, solution and guesses

            row = save.row;
            solution = save.solution;
            guess = save.guesses[row > 10 ? 9 : row];

            //loop over each guesses to update visuals
            for (int s = 0; s < save.guesses.Length; s++)
            {
                if (save.guesses[s] == null)
                {
                    continue;
                }

                //loop over all guess_grid children
                for (int i = 0; i < guess_grid.Children.Count; i++)
                {
                    var child = guess_grid.Children[i];
                    if (child.GetType() == typeof(Button))
                    {
                        Button button = (Button)child;
                        int buttonIndex = (int)button.GetValue(Grid.ColumnProperty);
                        if (buttonIndex <= 3 && (int)button.GetValue(Grid.RowProperty) == s)
                        {
                            int colorIndex = save.guesses[s][buttonIndex];
                            if(colorIndex >= 0)
                            {
                                button.BackgroundColor = colors[colorIndex];
                            }
                            else
                            {
                                button.BackgroundColor = Colors.Black;
                            }

                            if(s < row)
                            {
                                VisualizeFeedback(s, save.guesses[s]);
                            }
                        }
                    }
                }
            }
        }
    }

    //checks if you won the game
    private void WinCond()
    {
        //loops over all guesses to check if they are correct.
        for (int i = 0; i < guess.Length; i++)
        {
            if (guess[i] != solution[i])
            {
                return;
            }
        }
        //if guess is correct display the win message
        win_lbl.IsVisible = true;

        //sets row to stupidly large number so player cannot keep play after winning
        row = 9999;
    }

    //checks if row == 10 therefore you have no more guesses and you lose
    private void LoseCond()
    {
        if (row == 10)
        {
            lose_lbl.IsVisible = true;
        }
    }

    private void ResetBoard(object sender, EventArgs e)
    {
        guess = new int[4] { -1, -1, -1, -1 };
        GenerateSolution();
        row = 0;
        win_lbl.IsVisible = false;
        lose_lbl.IsVisible = false;

        //loop over all guess_grid children
        for (int i = 0; i < guess_grid.Children.Count; i++)
        {
            var child = guess_grid.Children[i];
            if (child.GetType() == typeof(BoxView))
            {
                BoxView boxView = (BoxView)child;
                if (boxView.Height == 20)
                {
                    boxView.Color = Colors.LightGrey;
                }
            }
            else if (child.GetType() == typeof(Button))
            {
                Button button = (Button)child;
                if ((int)button.GetValue(Grid.ColumnProperty) <= 3)
                {
                    button.BackgroundColor = Colors.Black;
                }
            }
        }
    }

    private void ExitBtn(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }

    //checks if the colors in the current row are the same as the solution
    private void CheckButton(object sender, EventArgs e)
    {
        //Cancel if not all guesses are set
        for (int i = 0; i < guess.Length; i++)
        {
            if (guess[i] == -1)
            {
                return;
            }
        }

        VisualizeFeedback(row, guess);

        //save the current guess in the game save
        save.guesses[row] = guess;

        //check win Cond
        WinCond();

        row++;
        guess = new int[4] { -1, -1, -1, -1 };

        //lose Cond
        LoseCond();
    }

    //cycles through the colours of each peg 
    private void CycleColor(object sender, EventArgs e)
	{
        Button btn = (Button)sender;
        int rowValue = Grid.GetRow(btn);
        int columnValue = Grid.GetColumn(btn);

        //Stop the code if the button we clicked on is not on the current (active) row
        if (rowValue != row)
        {
            return;
        }
        
        //index is the number associated with each item in an array(eg: [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.HotPink, Colors.Cyan]
        //                                                                   0            1               2           3               4               5)
            
        //get the current color
        Color color = btn.BackgroundColor;

        //find the index that belong to this color
        int index = Array.IndexOf(colors, color);

        //index + 1 (if index > 5 then index = 0)
        index++;
        if (index >= colors.Length)
        {
            index = 0;
        }

        //Update the color
        btn.BackgroundColor = colors[index];

        //Store the players guess
        guess[columnValue] = index;


  //      if (((Button)sender).BackgroundColor == Colors.Black)
  //      {
  //          ((Button)sender).BackgroundColor = Colors.Red;
  //      }
  //      else if (((Button)sender).BackgroundColor == Colors.Red)
  //      {
  //          ((Button)sender).BackgroundColor = Colors.Green;
  //      }
  //      else if (((Button)sender).BackgroundColor == Colors.Green)
  //      {
  //          ((Button)sender).BackgroundColor = Colors.Blue;
  //      }
  //      else if (((Button)sender).BackgroundColor == Colors.Blue)
  //      {
  //          ((Button)sender).BackgroundColor = Colors.Yellow;
  //      }
  //      else if (((Button)sender).BackgroundColor == Colors.Yellow)
  //      {
  //          ((Button)sender).BackgroundColor = Colors.HotPink;
  //      }
  //      else
  //      {
  //          ((Button)sender).BackgroundColor = Colors.Red;
  //      }
    }

    //start the game, disables the main menu and enables the grid
    private void StartGame(object sender, EventArgs e)
    {
        labels.IsVisible = true;
        labels.IsEnabled = true;
        guess_grid.IsVisible = true;
        guess_grid.IsEnabled = true;

        mainMenu.IsVisible = false;
        mainMenu.IsEnabled = false;

    }

    private void VisualizeFeedback(int targetedRow, int[] targetedGuess)
    {
        //temporarily store the correct boxviews
        BoxView[] boxviews = new BoxView[4];
        int boxviewIndex = 0;

        //loop over all guess_grid children
        for (int i = 0; i < guess_grid.Children.Count; i++)
        {
            var child = guess_grid.Children[i];

            //sort out the targeted boxviews
            //must be of type boxview
            if (child.GetType() == typeof(BoxView))
            {
                BoxView boxView = (BoxView)child;
                //must have the correct row
                //must NOT have the color black
                if ((int)boxView.GetValue(Grid.RowProperty) == targetedRow && boxView.Height == 20 && boxviewIndex < boxviews.Length)
                {
                    boxviews[boxviewIndex] = boxView;
                    boxviewIndex++;
                }
            }
        }

        //store how many times a color occurs in the solution
        int[] occurences = new int[colors.Length];
        for (int i = 0; i < solution.Length; i++)
        {
            occurences[solution[i]]++;
        }

        //Give feedback. First we check all the green cases because these have a higher priority then the orange cases (correct over wrong position checks)
        for (int i = 0; i < targetedGuess.Length; i++)
        {
            if (targetedGuess[i] == solution[i])
            {
                //correct = green pin
                boxviews[i].Color = Colors.Green;
                //remove occurence from the array
                occurences[solution[i]]--;
            }
        }

        //check for all orange cases. If the occurence is still not 0 (the same color is used on multiple places)
        for (int i = 0; i < targetedGuess.Length; i++)
        {
            if (targetedGuess[i] != solution[i])
            {
                int index = Array.IndexOf(solution, targetedGuess[i]);
                if (index != -1 && occurences[targetedGuess[i]] > 0)
                {
                    //almost correct = orange pin
                    boxviews[i].Color = Colors.Orange;
                    occurences[targetedGuess[i]]--;
                }
                else
                {
                    boxviews[i].Color = Colors.Black;
                }
            }
        }
    }
}

