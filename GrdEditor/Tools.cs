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

        public abstract void MouseDownHandler(MouseEventArgs args);
        public abstract void MouseUpHandler(MouseEventArgs args);

        protected MainForm _form;
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

                if (!FirstPointInited || SecondPointInited)
                {
                    // По-идее первая точка здесь всегда
                    // должна быть инициализирована, 
                    // а вторая - нет поэтому в целях отладки:
                    MessageBox.Show(String.Format(LftUpErrFmt, FirstPointInited, SecondPointInited));
                }
                else
                {
                    // Ну а тут наш клиент:
                    SecondPointInited = true;
                    SecondPoint = args.Location;
                }
            }
        }
    }
}
