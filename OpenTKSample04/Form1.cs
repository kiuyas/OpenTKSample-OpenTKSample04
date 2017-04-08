using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTKSample04
{

    public partial class Form1 : Form
    {
        private ViewPointParam param;

        private List<int> dataList;

        /// <summary>マウスドラッグ開始点X座標</summary>
        private int startX = -1;

        /// <summary>マウスドラッグ開始点Y座標</summary>
        private int startY = -1;

        /// <summary>マウスドラッグ開始時EyeX</summary>
        private float startEyeX = 0;

        /// <summary>マウスドラッグ開始時EyeY</summary>
        private float startEyeY = 0;

        /// <summary>マウスドラッグ開始時Alpha</summary>
        private float startAlpha = 0;

        /// <summary>マウスドラッグ開始時Beta</summary>
        private float startBeta = 0;

        public Form1()
        {
            InitializeComponent();
            param = new ViewPointParam();
            glControl1.MouseWheel += new MouseEventHandler(glControl1_MouseWheel);

            Random random = new Random();
            dataList = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                dataList.Add(random.Next(0, 100));
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GLControl canvas = sender as GLControl;

            // 初期化
            InitFor3D(canvas.Width, canvas.Height, param.Alpha, param.Beta);

            // 平面を描く
            DrawSurface();

            // グラフを描く
            DrawGraph(new Vector3(-2F, 0F, -1F));

            // 描画結果を反映させる
            canvas.SwapBuffers();
        }

        /// <summary>
        /// 三次元描画用に初期化する
        /// </summary>
        /// <param name="width">描画領域幅</param>
        /// <param name="height">描画領域高さ</param>
        private void InitFor3D(float width, float height, float alpha, float beta)
        {
            // クリア
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // ビューポートの設定
            GL.Viewport(0, 0, (int)width, (int)height);

            // 視点の設定
            Matrix4 modelView = Matrix4.LookAt(param.Eye, param.Target, param.Up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            // 射影の設定
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, width / height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            // 陰面処理をONにする
            GL.Enable(EnableCap.DepthTest);

            // 物体の回転
            if (alpha !=0 || beta != 0)
            {
                GL.Translate(-param.Eye.X, -param.Eye.Y, -param.Eye.Z);
                GL.Rotate(beta, new Vector3d(1, 0, 0));
                GL.Rotate(alpha, new Vector3d(0, 0, 1));
                GL.Translate(param.Eye.X, param.Eye.Y, param.Eye.Z);
            }
        }

        /// <summary>
        /// 平面を描く
        /// </summary>
        private void DrawSurface()
        {
            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);

            for (float i = -2; i <= 2.1; i += 0.1F)
            {
                // 水平方向
                GL.Vertex3(-2, i, 0);
                GL.Vertex3(2, i, 0);

                // 垂直方向
                GL.Vertex3(i, -2, 0);
                GL.Vertex3(i, 2, 0);
            }

            GL.End();

            GL.Begin(PrimitiveType.TriangleFan);

            GL.Vertex3(0, 1.95, 0);
            GL.Vertex3(-0.05, 1.85, 0);
            GL.Vertex3(0.05, 1.85, 0);

            GL.End();
        }

        private void DrawGraph(Vector3 offset)
        {
            Vector3 p1 = new Vector3(0, 0, 0);
            Vector3 p2 = new Vector3(4, 0, 2);

            GL.Color3(Color.LightGreen);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(p1.X + offset.X, p1.Y + offset.Y, p1.Z + offset.Z);
            GL.Vertex3(p2.X + offset.X, p1.Y + offset.Y, p1.Z + offset.Z);
            GL.Vertex3(p2.X + offset.X, p1.Y + offset.Y, p2.Z + offset.Z);
            GL.Vertex3(p1.X + offset.X, p1.Y + offset.Y, p2.Z + offset.Z);
            GL.End();

            float height = p2.Z - p1.Z;
            GL.Color3(Color.LightGreen);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(p1.X + offset.X, p1.Y + offset.Y, height / 2F + offset.Z);
            GL.Vertex3(p2.X + offset.X, p1.Y + offset.Y, height / 2F + offset.Z);
            GL.End();

            if (dataList != null)
            {
                GL.Color3(Color.Salmon);
                GL.Begin(PrimitiveType.LineStrip);
                float step = (p2.X - p1.X) / dataList.Count;
                for (int i = 0; i < dataList.Count; i++)
                {
                    float x = i * step + offset.X;
                    float y = p1.Y + offset.Y;
                    float z = height / 100 * dataList[i] + offset.Z;
                    GL.Vertex3(x, y, z);
                }
                GL.End();
            }
        }

        #region イベント処理

        private void glControl1_SizeChanged(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            float eyeZ = param.Eye.Z;
            eyeZ += e.Delta * -0.01F;
            param.Eye = new Vector3(param.Eye.X, param.Eye.Y, eyeZ);
            glControl1.Invalidate();
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            startX = e.X;
            startY = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                startEyeX = param.Eye.X;
                startEyeY = param.Eye.Y;
            }
            else if (e.Button == MouseButtons.Right)
            {
                startAlpha = param.Alpha;
                startBeta = param.Beta;
            }
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            int vx = e.X - startX;
            int vy = e.Y - startY;

            if (e.Button == MouseButtons.Left)
            {
                MoveEye(vx, vy);
            }
            else if (e.Button == MouseButtons.Right)
            {
                RotateObj(vx, vy);
                startX = e.X;
                startY = e.Y;
            }
        }

        private void MoveEye(int vx, int vy)
        {
            param.Eye = new Vector3(startEyeX - vx * 0.01F, startEyeY + vy * 0.01F, param.Eye.Z);
            param.Target = new Vector3(param.Eye.X, param.Eye.Y, param.Target.Z);
            glControl1.Invalidate();
        }

        private void RotateObj(int vx, int vy)
        {
            param.Alpha += vx;
            param.Beta += vy;

            glControl1.Invalidate();
        }

        #endregion

        /// <summary>
        /// リセットボタン押下時処理
        /// </summary>
        /// <param name="sender">イベント発生オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            param.ResetParameters();
            glControl1.Invalidate();
        }

        private void chkRotate_CheckedChanged(object sender, EventArgs e)
        {
            timRotate.Enabled = chkRotate.Checked;
        }

        private void timRotate_Tick(object sender, EventArgs e)
        {
            param.Alpha += 10F;
            if (param.Alpha > 360)
            {
                param.Alpha -= 360;
            }
            glControl1.Invalidate();
        }
    }
}
