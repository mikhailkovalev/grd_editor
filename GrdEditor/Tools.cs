using System;
using System.Drawing;
using System.Windows.Forms;

namespace GrdEditor
{
    public abstract class AbstractTool
    {
        public AbstractTool(MainForm form)
        {
            _form = form;
        }

        // Методы обработки событий мыши
        // Если метод вернул true, то
        // надо обновить экран
        public abstract void MouseDownHandler(MouseEventArgs args);
        public abstract void MouseUpHandler(MouseEventArgs args);
        public abstract void MouseMoveHandler(MouseEventArgs args);
        public abstract void Paint(Graphics g);

        protected MainForm _form;
        protected Pen _pen = new Pen(Color.Black);
    }

    public abstract class RectangleTool: AbstractTool
    {
        // Большинство инструментов носят "прямоугольный"
        // характер, поэтому рутину и однообразие вынесем
        // в отдельный класс

        public bool FirstPointInited = false;
        public bool SecondPointInited = false;

        public Point FirstPoint = new Point();
        public Point SecondPoint = new Point();

        const String LftDwnErrFmt = "Неожиданная конфигурация полей при нажатии ЛКМ:\nFirstPointInited = {0}\nSecondPointInited = {1}";
        const String LftUpErrFmt = "Неожиданная конфигурация полей при отпускании ЛКМ:\nFirstPointInited = {0}\nSecondPointInited = {1}";
        const String RghtDwnErrFmt = "Неожиданная конфигурация полей при нажатии ПКМ:\nFirstPointInited = {0}\nSecondPointInited = {1}";
        const String RghtUpErrFmt = "Неожиданная конфигурация полей при отпускании ПКМ:\nFirstPointInited = {0}\nSecondPointInited = {1}";

        public RectangleTool(MainForm form) : base(form) { }
        
        public override void MouseDownHandler(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                // Если обе точки без инициализации:
                // инициализируем первую
                // Иначе должны быть проинициализированны
                // обе точки. Это значит, что обе точки
                // должны быть сброшены и первую нужно
                // проинитить
                if (!FirstPointInited && !SecondPointInited)
                {
                    // FirstPointInited: было false, стало true
                    // SecondPointInited: было false и должно остаться false
                    FirstPointInited = true;
                }
                else if (FirstPointInited && SecondPointInited)
                {
                    // FirstPointInited: было true, и должно остаться true
                    // SecondPointInited: было true, стало false
                    SecondPointInited = false;
                }
                else
                {
                    // По-идее сюда мы попадать не должны, но
                    // исключительно в целях отладки:
                    MessageBox.Show(String.Format(LftDwnErrFmt, 
                                                  FirstPointInited,
                                                  SecondPointInited));
                    FirstPointInited = true;
                    SecondPointInited = false;
                }
                FirstPoint = args.Location;
            }
            else if (args.Button == MouseButtons.Right)
            {
                // Пусть клик правой кнопкой обрывает
                // процесс выделения

                if (!FirstPointInited && !SecondPointInited)
                {
                    // Если не проинициализирована ни одна точка
                    // то не делаем нихуя. Сам хз, зачем этот if
                }
                else if (FirstPointInited && !SecondPointInited)
                {
                    // О, наш клиент, как раз нужно оборвать выделение
                    FirstPointInited = false;
                }
                else if (FirstPointInited && SecondPointInited)
                {
                    // Тут как бы обрывать нечего: выделение уже
                    // завершено, поэтому пусть оно тупо сбрасывается
                    FirstPointInited = SecondPointInited = false;
                }
                else
                {
                    // По-идее сюда мы попадать не должны, но
                    // исключительно в целях отладки:
                    MessageBox.Show(String.Format(RghtDwnErrFmt, FirstPointInited, SecondPointInited));

                    // Ну и сбросим выделение, если оно тут когда-либо было:
                    FirstPointInited = SecondPointInited = false;
                }
            }
        }

        public override void MouseUpHandler(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                // Отпускание ЛКМ должно завершить
                // процесс выделения

                if (FirstPointInited && !SecondPointInited)
                {
                    // Ну а тут наш клиент:
                    SecondPointInited = true;
                    SecondPoint = args.Location;
                }
                else if (!FirstPointInited && !SecondPointInited)
                {
                    // Если выделение было сброшено ПКМ,
                    // но ЛКМ ещё не отпущена
                }
                else
                {
                    // По-идее первая точка здесь всегда
                    // должна быть инициализирована, 
                    // а вторая - нет поэтому в целях отладки:
                    MessageBox.Show(String.Format(LftUpErrFmt, FirstPointInited, SecondPointInited));
                }
            }
        }

        public override void MouseMoveHandler(MouseEventArgs args)
        {
            SecondPoint = args.Location;
        }

        public override void Paint(Graphics g)
        {
            if (FirstPointInited)
            {
                int x = Math.Min(FirstPoint.X, SecondPoint.X);
                int y = Math.Min(FirstPoint.Y, SecondPoint.Y);
                int w = Math.Abs(FirstPoint.X - SecondPoint.X);
                int h = Math.Abs(FirstPoint.Y - SecondPoint.Y);
                g.DrawRectangle(_pen, x, y, w, h);
            }
            //_pen.DashPattern = new Single[] { 3.0f, 3.0f };
            
        }
    }

    public class HandTool : RectangleTool
    {
        // Инструмент "рука" предназначен для
        // перетаскивания карты вдоль экрана

        public HandTool(MainForm form) : base(form) { }

        public override void MouseDownHandler(MouseEventArgs args)
        {
            base.MouseDownHandler(args);
            if (args.Button == MouseButtons.Left)
            {
                // Запоминаем старые значения
                // отображаемой области
                oldDownBound = _form.downBound;
                oldLeftBound = _form.leftBound;
                oldRightBound = _form.rightBound;
                oldUpBound = _form.upBound;

                CursorPos = args.Location;
                MapPos.X = _form.GetColumnFromXF(CursorPos.X);
                MapPos.Y = _form.GetRowFromYF(CursorPos.Y);
            }
            else if (args.Button == MouseButtons.Right)
            {
                // Если "выделение" было сброшено, возвращаем
                // старые границы на место
                _form.downBound = oldDownBound;
                _form.leftBound = oldLeftBound;
                _form.rightBound = oldRightBound;
                _form.upBound = oldUpBound;
                _form.Update(CursorPos, MapPos);
            }
        }
        
        public override void MouseUpHandler(MouseEventArgs args)
        {
            base.MouseUpHandler(args);

            if (args.Button == MouseButtons.Left && FirstPointInited && SecondPointInited)
            {
                _form.Update(args.Location, MapPos);
            }
            
            // Так как после перетаскивания мы больше ничего
            // не хотим делать с обозначенными двумя точками,
            // то мы их тупо сбрасываем
            FirstPointInited = SecondPointInited = false;
        }

        public override void MouseMoveHandler(MouseEventArgs args)
        {
            base.MouseMoveHandler(args);
            if (FirstPointInited && !SecondPointInited)
            {
                Int32 dx = args.X - FirstPoint.X;
                Int32 dy = args.Y - FirstPoint.Y;
                _form.leftBound = oldLeftBound + dx;
                _form.rightBound = oldRightBound + dx;
                _form.upBound = oldUpBound + dy;
                _form.downBound = oldDownBound + dy;
                _form.Update(args.Location, MapPos);
            }
        }

        public override void Paint(Graphics g)
        {
            
        }

        private Int32 oldLeftBound, oldRightBound;
        private Int32 oldUpBound, oldDownBound;
        private Point CursorPos;
        private PointF MapPos;
    }
}
