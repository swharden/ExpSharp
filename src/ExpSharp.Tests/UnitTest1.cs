namespace ExpSharp.Tests;

public class Tests
{
    [Test]
    public void Test1()
    {
        List<Point> points = [
            new(0.00, 50.41),
            new(0.05, 44.89),
            new(0.10, 40.41),
            new(0.35, 26.92),
            new(0.40, 25.31),
            new(0.45, 23.43),
            new(0.50, 22.18),
            new(0.55, 20.86),
            new(0.60, 19.44),
            new(0.65, 17.98),
            new(0.70, 17.28),
            new(0.75, 16.80),
            new(1.10, 13.37),
            new(1.15, 12.83),
            new(1.20, 12.28),
            new(1.25, 12.29),
            new(1.55, 10.98),
            new(1.60, 10.55),
            new(1.65, 10.15),
            new(1.70, 9.83),
            new(1.75, 9.43),
            new(1.80, 8.82),
            new(1.85, 8.60),
            new(2.20, 8.27),
            new(2.25, 8.17),
            new(2.30, 8.41),
            new(2.35, 8.37),
            new(2.40, 8.67),
            new(2.45, 8.65),
            new(2.50, 8.61),
        ];

        Fitter fitter = new();
        fitter.Fit(points);

        ScottPlot.Plot plot = new();
        ScottPlot.Coordinates[] coordinates = points.Select(pt => new ScottPlot.Coordinates(pt.X, pt.Y)).ToArray();
        var markers = plot.Add.Markers(coordinates);

        List<ScottPlot.Coordinates> fittedCoordinates = [];
        for (double x = 0; x <= 2.5; x += 0.1)
        {
            fittedCoordinates.Add(new(x, fitter.GetY(x)));
        }

        var sp = plot.Add.ScatterLine(fittedCoordinates);

        plot.Title($"{fitter.Formula}\nFit achieved in {fitter.FitTime.TotalMilliseconds:N2} ms");

        plot.SavePng("test.png", 800, 600);
    }
}