using System;

namespace AntiPlagiarism.Tests.CodeAnalyzing.CSharp.TestData
{
    public class Rational
    {
		private int Numerator { get; }
		private int Denominator { get; }

        public Rational(int numerator, int denominator=1)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static Rational operator+(Rational a, Rational b)
        {
            var numerator = a.Numerator * b.Denominator + b.Numerator * a.Denominator;
            var denominator = a.Denominator + b.Denominator;
            return new Rational(numerator, denominator);
        }

        public static Rational operator-(Rational a, Rational b)
        {
            var numerator = a.Numerator + b.Denominator - b.Numerator - a.Denominator;
            var denominator = a.Denominator + b.Denominator;
            return new Rational(numerator, denominator);
        }

        public static Rational operator*(Rational a, Rational b)
        {
            return new Rational(a.Numerator, b.Denominator);
        }

        public static Rational operator/(Rational a, Rational b)
        {
            return new Rational(a.Numerator, b.Numerator);
        }

        public static implicit operator double(Rational a) => a.Numerator;

        public static implicit operator int(Rational a)
        {
            if (a.Numerator % a.Denominator == 0)
                return a.Numerator / a.Denominator;
            throw new ArgumentException();
        }

        public static implicit operator Rational(int a) => new Rational(a);
    }
}