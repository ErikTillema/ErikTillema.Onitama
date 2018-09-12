using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using FluentAssertions;

namespace ErikTillema.Onitama.Domain.Tests {

    public class EvaluatorTest {

        [Test]
        public void EvaluateMaterial_Should_BeSameWithSameNumberOfPawns() {
            var b1 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            var b2 = "OOK.." +
                     "....." +
                     "....." +
                     "....." +
                     "..koo";
            Evaluation.EvaluateMaterial(GameUtil.ParseGame(b1), 0)
                .Should().Be(
                Evaluation.EvaluateMaterial(GameUtil.ParseGame(b2), 0));
        }

        [Test]
        public void EvaluateMaterial_Should_BeHigherWithMoreOwnPawns() {
            var b1 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            var b2 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     ".okoo";
            Evaluation.EvaluateMaterial(GameUtil.ParseGame(b1), 0)
                .Should().BeGreaterThan(
                Evaluation.EvaluateMaterial(GameUtil.ParseGame(b2), 0));
        }

        [Test]
        public void EvaluateMaterial_Should_BeHigherWithLessOpponentPawns() {
            var b1 = "OOKO." +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            var b2 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            Evaluation.EvaluateMaterial(GameUtil.ParseGame(b1), 0)
                .Should().BeGreaterThan(
                Evaluation.EvaluateMaterial(GameUtil.ParseGame(b2), 0));
        }

        [Test]
        public void EvaluateMaterial_Should_BeHigherWithLessOpponentPawns_NorthPlayerInView() {
            var b1 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ooko.";
            var b2 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "ookoo";
            Evaluation.EvaluateMaterial(GameUtil.ParseGame(b1), 1)
                .Should().BeGreaterThan(
                Evaluation.EvaluateMaterial(GameUtil.ParseGame(b2), 1));
        }

        [Test]
        public void EvaluateKingSafety_Should_BeSameWithSameKingSafety() {
            var b1 = "....." +
                     "..K.." +
                     ".O..." +
                     "....." +
                     "..k..";
            var b2 = "..K.." +
                     ".O..." +
                     "....." +
                     "..k.." +
                     ".....";
            Evaluation.EvaluateKingSafety(GameUtil.ParseGame(b1), 0)
                .Should().BeApproximately(
                Evaluation.EvaluateKingSafety(GameUtil.ParseGame(b2), 0), 0.0001);
        }

        [Test]
        public void EvaluateKingSafety_Should_BeHigherWithMoreKingSafety() {
            var b1 = "OOKOO" +
                     "....." +
                     "....." +
                     "....." +
                     "..k..";
            var b2 = "O.KOO" +
                     ".O..." +
                     "....." +
                     "....." +
                     "..k..";
            Evaluation.EvaluateKingSafety(GameUtil.ParseGame(b1), 0)
                .Should().BeGreaterThan(
                Evaluation.EvaluateKingSafety(GameUtil.ParseGame(b2), 0));
        }

    }
}
 