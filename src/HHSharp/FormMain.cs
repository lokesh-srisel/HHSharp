﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HHSharp
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            btnRun_Click(null, null);
            formsPlot1.plt.mouseTracker.lowQualityWhileInteracting = false;
            formsPlot2.plt.mouseTracker.lowQualityWhileInteracting = false;
        }

        private double[] GenerateStimulusWaveform(int pointCount)
        {
            double[] stimulus = new double[pointCount];

            if (rbStimConstant.Checked)
            {
                for (int i = 0; i < pointCount; i++)
                    stimulus[i] = (double)nudStimCurrent.Value;
            }
            else if (rbStimSquare.Checked)
            {
                int indexStart = (int)(pointCount * .33);
                int indexEnd = (int)(pointCount * .66);
                for (int i = indexStart; i < indexEnd; i++)
                    stimulus[i] = (double)nudStimCurrent.Value;
            }
            else if (rbStimRamp.Checked)
            {
                for (int i = 0; i < pointCount; i++)
                    stimulus[i] = (double)nudStimCurrent.Value * i / pointCount;
            }
            else
            {
                throw new Exception("unknown stimulus waveform");
            }

            return stimulus;
        }

        private void RunSimulation()
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            double stepSizeMsec = .01;
            double sampleRate = 1.0 / stepSizeMsec;
            double simulationLengthMsec = (double)nudDurationMs.Value;
            int pointCount = (int)(simulationLengthMsec / stepSizeMsec);

            double[] voltage = new double[pointCount];
            double[] stateH = new double[pointCount];
            double[] stateM = new double[pointCount];
            double[] stateN = new double[pointCount];

            double[] stimulus = GenerateStimulusWaveform(pointCount);

            var hh = new HHModel
            {
                ENa = (double)nudENa.Value,
                EK = (double)nudEK.Value,
                EKleak = (double)nudEKLeak.Value,
                gNa = (double)nudGNa.Value,
                gK = (double)nudGK.Value,
                gKleak = (double)nudGKLeak.Value,
                Cm = (double)nudCm.Value
            };

            for (int i = 0; i < pointCount; i++)
            {
                hh.Iterate(stimulus[i], stepSizeMsec);
                voltage[i] = hh.Vm;
                stateH[i] = hh.h.state;
                stateM[i] = hh.m.state;
                stateN[i] = hh.n.state;
            }

            double elapsedSec = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            string message = string.Format("simulation completed in {0:0.00} ms ({1:0.00} Hz)", elapsedSec * 1000.0, 1 / elapsedSec);
            Debug.WriteLine(message);

            formsPlot1.plt.Clear();
            formsPlot1.plt.PlotSignal(voltage, sampleRate, yOffset: -70);
            formsPlot1.plt.AxisAuto();
            formsPlot1.plt.YLabel("Membrane Potential (mV)");
            formsPlot1.plt.XLabel("Simulation Time (milliseconds)");
            formsPlot1.Render();

            formsPlot2.plt.Clear();
            formsPlot2.plt.PlotSignal(stateH, sampleRate, label: "h");
            formsPlot2.plt.PlotSignal(stateM, sampleRate, label: "m");
            formsPlot2.plt.PlotSignal(stateN, sampleRate, label: "m");
            formsPlot2.plt.Legend();
            formsPlot2.plt.AxisAuto();
            formsPlot2.plt.YLabel("Channel State");
            formsPlot2.plt.XLabel("Simulation Time (milliseconds)");
            formsPlot2.Render();
        }

        private void btnRun_Click(object sender, EventArgs e) { RunSimulation(); }
        private void nudStimCurrent_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void rbStimConstant_CheckedChanged(object sender, EventArgs e) { RunSimulation(); }
        private void rbStimSquare_CheckedChanged(object sender, EventArgs e) { RunSimulation(); }
        private void rbStimRamp_CheckedChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudCm_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudResolutionMs_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudDurationMs_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudGKLeak_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudGK_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudGNa_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudEKLeak_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudEK_ValueChanged(object sender, EventArgs e) { RunSimulation(); }
        private void nudENa_ValueChanged(object sender, EventArgs e) { RunSimulation(); }

        private void formsPlot1_MouseClicked(object sender, MouseEventArgs e)
        {
            formsPlot2.plt.MatchAxis(formsPlot1.plt, horizontal: true, vertical: false);
            formsPlot2.Render();
        }

        private void formsPlot2_MouseClicked(object sender, MouseEventArgs e)
        {
            formsPlot1.plt.MatchAxis(formsPlot2.plt, horizontal: true, vertical: false);
            formsPlot1.Render();
        }

        private void label6_MouseEnter(object sender, EventArgs e)
        {
            lblUrl.Font = new Font(lblUrl.Font.Name, lblUrl.Font.SizeInPoints, FontStyle.Underline);
        }

        private void label6_MouseLeave(object sender, EventArgs e)
        {
            lblUrl.Font = new Font(lblUrl.Font.Name, lblUrl.Font.SizeInPoints, FontStyle.Regular);
        }

        private void lblUrl_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/swharden/HHSharp");
        }
    }
}