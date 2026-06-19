using Kompas6Constants;
using KompasAPI7;

namespace ConsoleApp1
{

    internal class Kompas_Plagin
    {

        static void Main(string[] args)
        {


            double x = 500;
            double y = 1000;
            double x_zero = 125;
            double y_zero = 125;

            IApplication app;
            Type kompastype = Type.GetTypeFromProgID("KOMPAS.Application.7");
            app = (IApplication)Activator.CreateInstance(kompastype);
            app.Visible = true;

            IDocuments docs = app.Documents;
            IKompasDocument doc = docs.Add(DocumentTypeEnum.ksDocumentDrawing, true);
            IKompasDocument2D doc2D = (IKompasDocument2D)doc;
            IViewsAndLayersManager manager_vidov = doc2D.ViewsAndLayersManager;

            IViews vidi = manager_vidov.Views;
            IView Sector_vid = vidi.Add(LtViewType.vt_Normal);
            Sector_vid.X = 0;
            Sector_vid.Y = 0;
            Sector_vid.Name = "Сектор катушки";
            Sector_vid.Update();

            //контейнер для выносок
            ISymbols2DContainer symbols = (ISymbols2DContainer)Sector_vid;
            ILeaders leaders = symbols.Leaders;


            IView vid = vidi.Add(LtViewType.vt_Normal);
            vid.Scale = 0.2;
            vid.X = 0;
            vid.Y = 0;
            vid.Name = "Катушка";
            vid.Update();
            IDrawingContainer container_sector = (IDrawingContainer)Sector_vid;
            IArcs arcs = container_sector.Arcs;
            IDrawingContainer drawingContainer = (IDrawingContainer)vid;
            ICircles krugi = drawingContainer.Circles;

            Console.WriteLine("введите количество слоев:");
            int count_layers = int.Parse(Console.ReadLine());

            double[] layers = new double[count_layers];
            Console.WriteLine("Введите количество реек:");
            int countslat = int.Parse(Console.ReadLine());
            double angle = 360 - (360 / countslat);
            Console.WriteLine("введите наружний диаметр цилиндра:");
            int outerdiam = int.Parse(Console.ReadLine());
            //круг наружний
            ICircle innerdiam = krugi.Add();
            innerdiam.Xc = x;
            innerdiam.Yc = y;
            innerdiam.Radius = outerdiam / 2;
            innerdiam.Update();
            //круг внутренний
            ICircle outerdiamC = krugi.Add();
            outerdiamC.Xc = x;
            outerdiamC.Yc = y;
            outerdiamC.Radius = (outerdiam - 6) / 2;
            outerdiamC.Update();
            //дуга внутренняя
            IArc arcin = arcs.Add();
            arcin.X2 = -((outerdiam / 2) - x_zero);
            arcin.Y2 = y_zero;
            arcin.Angle1 = angle;
            arcin.Radius = (outerdiam / 2) - 3;
            arcin.Direction = true;
            arcin.Update();
            //дуга наружняя
            IArc arcout = arcs.Add();
            arcout.X2 = -((outerdiam / 2) - x_zero);
            arcout.Y2 = y_zero;
            arcout.Angle1 = angle;
            arcout.Radius = outerdiam / 2;
            arcout.Direction = true;
            arcout.Update();

            double XC = arcin.Xc;
            double YC = arcin.Yc;
            double full_diam = outerdiam;
            ILineSegments line_sector = (ILineSegments)container_sector.LineSegments;
            for (int i = 0; i < count_layers; i++)
            {
                Console.WriteLine("Слой номер " + (i + 1) + ". Введите толщину материала:");
                string rep = Console.ReadLine();

                bool isHacth = false;
                bool isSlat = false;
                if (rep.Contains("+"))
                {
                    rep = "1,5";
                }
                if (rep.Contains("-"))
                {
                    isHacth = true;
                    rep = rep.Replace("-", "");
                }
                if (rep.Contains("/"))
                {
                    isSlat = true;
                    rep = rep.Replace("/", "");
                }
                string v = rep.Replace(".", ",");
                layers[i] = double.Parse(v);
                full_diam = full_diam + (layers[i] * 2);
                ICircle krug = krugi.Add();
                krug.Style = 2;
                krug.Xc = x;
                krug.Yc = y;
                krug.Radius = full_diam / 2;
                krug.Update();

                IArc arc = arcs.Add();
                arc.Radius = full_diam / 2;
                arc.Direction = true;
                arc.Xc = XC;
                arc.Yc = YC;
                arc.Angle2 = angle;
                arc.Y1 = YC;
                arc.Angle1 = 0;
                arc.Update();
                if (isHacth == true)
                {
                    IArc prev_arc = (IArc)arcs[i + 2];
                    prev_arc.Style = 3;
                    prev_arc.Update();
                }

                /*рисуем выноску
                if (isSlat)
                {
                    IBaseLeader base_lead = leaders.Add(DrawingObjectTypeEnum.ksDrLeader);
                    ksDrawingObjectParamTypeEnum param = base_lead.DrawingObjectParamType;
                    param.
                    base_lead.SetBranchTextPosition(10, 10);
                    ILeader lead = (ILeader)base_lead;
                    lead.SetSignPosition(0, 30, 30);
                    base_lead.Update();

                }
                */

                if (i + 1 == count_layers)
                {
                    //замыкающие прямые в секторе
                    ILineSegment line_sec_hor = line_sector.Add();
                    line_sec_hor.X1 = arcin.X1;
                    line_sec_hor.Y1 = arcin.Y1;
                    line_sec_hor.X2 = arc.X1;
                    line_sec_hor.Y2 = arc.Y1;
                    line_sec_hor.Update();
                    ILineSegment line_sec_vert = line_sector.Add();
                    line_sec_vert.X1 = arcin.X2;
                    line_sec_vert.Y1 = arcin.Y2;
                    line_sec_vert.X2 = arc.X2;
                    line_sec_vert.Y2 = arc.Y2;
                    line_sec_vert.Update();
                }

            }
            //штрихуем
            int count_arcs = arcs.Count;
            IHatches hatches = container_sector.Hatches;
            for (int i = count_arcs - 1; i >= 2; i--)
            {
                IArc arc_hatch = (IArc)arcs[i];

                if (arc_hatch.Style == 3)
                {
                    IHatch hatch = hatches.Add();
                    IHatchParam hatchParam = (IHatchParam)hatch;
                    hatchParam.Style = 1;
                    IBoundariesObject boundaries = (IBoundariesObject)hatch;
                    boundaries.AddBoundaries(arc_hatch, false);
                    IArc arc_previous = (IArc)arcs[i - 1];
                    boundaries.AddBoundaries(arc_hatch, false);
                    boundaries.AddBoundaries(arc_previous, false);
                    ILineSegment lineup = (ILineSegment)line_sector[0];
                    ILineSegment linedown = (ILineSegment)line_sector[1];
                    boundaries.AddBoundaries(lineup, false);
                    boundaries.AddBoundaries(linedown, false);
                    hatch.Update();
                    arc_hatch.Style = 1;
                    arc_hatch.Update();
                }
            }



            //черчение массива линий
            //начальная рейка
            ILineSegments lini = (ILineSegments)drawingContainer.LineSegments;
            ILineSegment oci = lini.Add();
            oci.X1 = x;
            oci.Y1 = (y + (outerdiam - 6) / 2) - 15;
            oci.X2 = x;
            oci.Y2 = (y + full_diam / 2) + 15;
            oci.Style = 3;
            oci.Update();
            //массив реек
            double r_start = (outerdiam / 2) - 15;
            double r_end = (full_diam / 2) + 15;
            for (int i = 0; i < countslat; i++)
            {
                double step = (2 * Math.PI / countslat) * i;
                double x1 = x - (r_start * Math.Sin(step));
                double y1 = y + (r_start * Math.Cos(step));

                double x2 = x - (r_end * Math.Sin(step));
                double y2 = y + (r_end * Math.Cos(step));
                ILineSegment line = lini.Add();
                line.X1 = x1;
                line.Y1 = y1;
                line.X2 = x2;
                line.Y2 = y2;
                line.Style = 3;
                line.Update();
            }
            // осевые по центру
            //вертикальная
            ILineSegment center_vertical = lini.Add();
            center_vertical.X1 = x;
            center_vertical.Y1 = y - 50;
            center_vertical.X2 = x;
            center_vertical.Y2 = y + 50;
            center_vertical.Style = 3;
            center_vertical.Update();
            //горизонатльная
            ILineSegment center_horizon = lini.Add();
            center_horizon.X1 = x - 50;
            center_horizon.Y1 = y;
            center_horizon.X2 = x + 50;
            center_horizon.Y2 = y;
            center_horizon.Style = 3;
            center_horizon.Update();
            //размерная надпись
            ISymbols2DContainer Razms = (ISymbols2DContainer)vid;
            ILineDimensions LineDims = Razms.LineDimensions;
            ILineDimension LineDim = LineDims.Add();
            LineDim.Orientation = (ksLineDimensionOrientationEnum)0;
            LineDim.X1 = x;
            LineDim.Y1 = y + full_diam / 2;
            LineDim.X2 = x - ((full_diam / 2) * Math.Sin(2 * Math.PI / countslat));
            LineDim.Y2 = y + ((full_diam / 2) * Math.Cos(2 * Math.PI / countslat));
            LineDim.X3 = LineDim.X1 - (LineDim.X1 - LineDim.X2) / 2;
            LineDim.Y3 = LineDim.Y1 - ((LineDim.Y1 - LineDim.Y2) / 2) + 20;
            LineDim.Update();


        }
    }
}
