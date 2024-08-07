using System.Diagnostics;

namespace ExpSharp;

public class Fitter
{
    public double InitialA { get; set; } = 1;
    public double InitialB { get; set; } = 1;
    public double InitialC { get; set; } = 1;

    public double A { get; private set; } = double.NaN;
    public double B { get; private set; } = double.NaN;
    public double C { get; private set; } = double.NaN;
    public double Error { get; private set; } = double.NaN;

    public string Formula => double.IsNaN(A)
        ? "Fit not achieved"
        : $"Y = {(float)A} + {(float)B} * e^({(float)C} * x)";

    public double TotalErrorThreshold { get; set; } = 0.01;
    public int MaxIterations { get; set; } = 10_000;

    // TODO: wrap this in a class
    public TimeSpan FitTime { get; private set; } = TimeSpan.Zero;
    //public int FitIterations { get; private set; } = 0;


    public void Fit(List<Point> points)
    {
        Stopwatch sw = Stopwatch.StartNew();
        ExpCoeffsAfterDescent coeffs = FollowGradient(points);
        A = coeffs.A;
        B = coeffs.B;
        C = coeffs.C;
        Error = coeffs.Error;
        FitTime = sw.Elapsed;
    }

    readonly record struct ExpCoeffs(double A, double B, double C);

    readonly record struct ExpCoeffsAfterDescent(double A, double B, double C, double Error)
    {
        public ExpCoeffs Coeffs => new(A, B, C);
    }

    public double GetY(double x)
    {
        return GetY(x, A, B, C);
    }

    public static double GetY(double x, double a, double b, double c)
    {
        return a + b * Math.Exp(c * x);
    }

    private static double ErrorSquared(List<Point> pts, double a, double b, double c)
    {
        double total_error = 0;
        for (int i = 0; i < pts.Count; i++)
        {
            double new_error = pts[i].Y - GetY(pts[i].X, a, b, c);
            if (total_error > 1E+15) break;
            total_error += new_error * new_error;
        }

        return total_error;
    }

    private ExpCoeffsAfterDescent FollowGradient(List<Point> pts)
    {
        double minDistance = 0.0000001;
        double distance = 0.25;

        double a = InitialA;
        double b = InitialB;
        double c = InitialC;
        double previousError = ErrorSquared(pts, a, b, c);

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            double na = 0;
            double nb = 0;
            double nc = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                na += 2 * -(pts[i].Y - GetY(pts[i].X, a, b, c));
                nb += 2 * (pts[i].Y - GetY(pts[i].X, a, b, c)) * -Math.Exp(pts[i].X * c);
                nc += 2 * (pts[i].Y - GetY(pts[i].X, a, b, c)) * -(b * Math.Exp(c * pts[i].X) * pts[i].X);
            }

            double length = Math.Sqrt(na * na + nb * nb + nc * nc);
            if (length < minDistance)
            {
                return new(a, b, c, previousError);
            }

            na = -na / length;
            nb = -nb / length;
            nc = -nc / length;

            double va = na * distance;
            double vb = nb * distance;
            double vc = nc * distance;

            double newError = ErrorSquared(pts, a + va, b + vb, c + vc);

            while (newError > previousError)
            {
                distance /= 2;
                //Console.WriteLine($"[{iteration}] too far, backing up");
                if (distance < minDistance)
                {
                    //Console.WriteLine($"[{iteration}] min distance reached");
                    return new(a, b, c, previousError);
                }
                va = na * distance;
                vb = nb * distance;
                vc = nc * distance;
                newError = ErrorSquared(pts, a + va, b + vb, c + vc);
            }

            //Console.WriteLine($"[{iteration}] stepping downward");
            a += va;
            b += vb;
            c += vc;
            previousError = newError;

            if (previousError < TotalErrorThreshold)
            {
                //Console.WriteLine($"[{iteration}] error threshold reached");
                return new(a, b, c, previousError);
            }
        }

        //Console.WriteLine($"max iterations reached");
        return new(a, b, c, previousError);
    }
}
