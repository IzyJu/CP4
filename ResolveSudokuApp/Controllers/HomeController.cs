using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ninject.Activation;
using ResolveSudokuApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ResolveSudokuApp.Controllers
{
    public class HomeController : Controller
    {
        private static string[] _boxes;
        private static string _cols;
        private static string _rows;
        private static SortedList<string, string[]> _peers;
        private static List<string[]> _unitList;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ResolvedSudo(string input1, string input2, string input3, string input4, string input5, string input6, string input7, string input8, string input9, string input10, string input11, string input12, string input13, string input14, string input15, string input16, string input17, string input18, string input19, string input20, string input21, string input22, string input23, string input24, string input25, string input26, string input27, string input28, string input29, string input30, string input31, string input32, string input33, string input34, string input35, string input36, string input37, string input38, string input39, string input40, string input41, string input42, string input43, string input44, string input45, string input46, string input47, string input48, string input49, string input50, string input51, string input52, string input53, string input54, string input55, string input56, string input57, string input58, string input59, string input60, string input61, string input62, string input63, string input64, string input65,string input66,string input67, string input68, string input69, string input70, string input71, string input72, string input73, string input74, string input75, string input76, string input77, string input78, string input79, string input80, string input81)
        {
            string[] puzzle = { input1, input2, input3, input4, input5, input6, input7, input8, input9, input10, input11, input12, input13, input14, input15, input16, input17, input18, input19, input20, input21, input22, input23, input24, input25, input26, input27, input28, input29, input30, input31, input32, input33, input34, input35, input36, input37, input38, input39, input40, input41, input42, input43, input44, input45, input46, input47, input48, input49, input50, input51, input52, input53, input54, input55, input56, input57, input58, input59, input60, input61, input62, input63, input64, input65, input66, input67, input68, input69, input70, input71, input72, input73, input74, input75, input76, input77, input78, input79, input80, input81 };

            string puzzleString = "";
            for (int i = 0; i < puzzle.Length; i++)
            {
                if (puzzle[i] == null)
                {
                    puzzle[i] = ".";
                }
                puzzleString += puzzle[i];
            }
            _cols = "123456789";
            _rows = "ABCDEFGHI";
            _boxes = Cross(_rows, _cols);

            var rowUnits = new List<string[]>();
            foreach (var c in _cols)
                rowUnits.Add(Cross(_rows, c.ToString()));

            var colUnits = new List<string[]>();
            foreach (var r in _rows)
                colUnits.Add(Cross(r.ToString(), _cols));

            var squareUnits = new List<string[]>();
            foreach (var rs in new[] { "ABC", "DEF", "GHI" })
                squareUnits.AddRange(new[] { "123", "456", "789" }.Select(cs => Cross(rs, cs)));

            _unitList = new List<string[]>();
            _unitList.AddRange(rowUnits);
            _unitList.AddRange(colUnits);
            _unitList.AddRange(squareUnits);

            var units = new SortedList<string, string[][]>();
            foreach (var s in _boxes)
                units.Add(s, _unitList.Where(x => x.Contains(s)).ToArray());

            _peers = new SortedList<string, string[]>();

            foreach (var s in _boxes)
            {
                var peer = new List<string>();
                foreach (var row in units[s])
                {
                    var elemStrings = row.Where(x => x != s).ToArray();
                    foreach (var elem in elemStrings)
                    {
                        if (!peer.Contains(elem))
                            peer.Add(elem);
                    }
                }
                _peers.Add(s, peer.ToArray());
            }
            Eliminate(GridValuesExtended(puzzleString));
            var final = Search(GridValuesExtended(puzzleString));
            return View(final);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static string[] Cross(string a, string b)
        {
            var results = new List<string>();

            foreach (var charA in a)
                foreach (var charB in b)
                    results.Add(charA + "" + charB);

            return results.ToArray();
        }
        private static SortedList<string, string> Eliminate(SortedList<string, string> values)
        {
            var solvedValues = values.Keys.Where(box => values[box].Length == 1).ToList();

            foreach (var box in solvedValues)
            {
                var digit = values[box];
                foreach (var peer in _peers[box])
                {
                    if (digit != "")
                        values[peer] = values[peer].Replace(digit, "");
                }
            }

            return values;
        }
        private static SortedList<string, string> GridValuesExtended(string grid)
        {
            var values = new List<string>();
            string alldigits = "123456789";

            foreach (var c in grid)
            {
                if (c == '.')
                    values.Add(alldigits);
                else if (alldigits.Contains(c))
                    values.Add("" + c);
            }

            if (grid.Length != 81) return null;

            var dict = new SortedList<string, string>();
            foreach (var item in _boxes.Zip(values, (a, b) => new { Box = a, Grid = b }))
                dict.Add(item.Box, item.Grid);

            return dict;
        }
        private static SortedList<string, string> OnlyChoice(SortedList<string, string> values)
        {
            foreach (var unit in _unitList)
            {
                foreach (var digit in "123456789")
                {
                    var dplaces = unit.Where(box => values[box].Contains(digit)).ToList();

                    if (dplaces.Count == 1)
                        values[dplaces[0]] = digit.ToString();
                }
            }

            return values;
        }
        private static SortedList<string, string> ReducePuzzle(SortedList<string, string> values)
        {
            var stalled = false;

            while (!stalled)
            {
                var solvedValuesBefore = values.Keys.Count(box => values[box].Length == 1);

                if (!values.Values.Any(x => x.Length > 1))
                {
                    stalled = true;
                    continue;
                }

                values = Eliminate(values);
                //Display(values);
                //Console.WriteLine();

                if (!values.Values.Any(x => x.Length > 1))
                {
                    stalled = true;
                    continue;
                }

                values = OnlyChoice(values);
                //Display(values);
                //Console.WriteLine();

                var solvedValuesAfter = values.Keys.Count(box => values[box].Length == 1);

                stalled = solvedValuesBefore == solvedValuesAfter;

                if (values.Keys.Count(box => values[box].Length == 0) > 0)
                    return null;
            }

            return values;
        }
        private static SortedList<string, string> Search(SortedList<string, string> values)
        {
            values = ReducePuzzle(values);

            if (values == null)
                return null;

            if (_boxes.Select(s => values[s].Length == 1).All(x => x))
                return values; // solved

            var pairs = new SortedList<string, int>();
            foreach (var s in _boxes)
            {
                if (values[s].Length > 1)
                    pairs.Add(s, values[s].Length);
            }

            string boxWithMinLength = null;
            int minLength = 0;
            foreach (var pair in pairs)
            {
                if (boxWithMinLength == null)
                {
                    boxWithMinLength = pair.Key;
                    minLength = pair.Value;
                }
                else
                {
                    if (pair.Value < minLength)
                    {
                        boxWithMinLength = pair.Key;
                        minLength = pair.Value;
                    }
                }
            }

            foreach (var value in values[boxWithMinLength ?? throw new InvalidOperationException()])
            {
                var newSudoku = new SortedList<string, string>();

                foreach (var x in values)
                    newSudoku.Add(x.Key, x.Value);

                newSudoku[boxWithMinLength] = "" + value;
                var attempt = Search(newSudoku);

                if (attempt != null)
                    return attempt;
            }

            return null;
        }
    }
}
