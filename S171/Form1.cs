using System;
using System.Drawing;
using System.Windows.Forms;
using canlibCLSNET;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace S171
{
    public partial class vForm1 : Form
    {
        public vForm1()
        {
            InitializeComponent();
        }

        public bool Kavser_Can_Init_Flag = false;
        public int handl = 0;
        public int Kavser_Can_bus_freq = Canlib.canBITRATE_1M;
        public int Kavser_Can_bus_tseg1 = 5;
        public int Kavser_Can_bus_tseg2 = 2;
        public int Kavser_Can_bus_sjw = 1;
        public int Kavser_Can_bus_drivertype = Canlib.canDRIVER_NORMAL;
        public int Kavser_Can_bus_noSamp = 1;
        public int RX_ID = 0x320;

        int[] BcmPin = new int[4];
        int[] BcmKey = new int[6];
        int[] BcmRan = new int[5];
        /************        数字转化为16进制    ****************************/





        #region  can初始化函数
        private void Kvaser_Can_Init()
        {
            int channel_count = 0;//定义CAN通道变
            Canlib.canUnloadLibrary();//如果您动态加载CANLIB32.DLL（即使用Win32 API LoadLibrary）并需要使用Win32 API卸载它，请使用此函数FreeLibrary。canUnloadLibrary（）将释放分配的内存，卸载canlib32.dll已加载的DLL并取消初始化数据结构。您必须再次调用canInitializeLibrary（）以使用canlib32.dll中的API函数。
                                      //暂时没有什么用的
            Canlib.canInitializeLibrary();//会初始化驱动函数程序
            if (Canlib.canGetNumberOfChannels(out channel_count) == Canlib.canStatus.canOK)//该功能返回计算机中可用CAN通道的数量。虚拟频道包含在这个数字中,默认是的虚拟通道0
                                                                                           //找到 Canlib.canStatus.canOK这句话的意思????
                                                                                           //判断有CAN通道，并且得到CAN通道的数量

            {
                comboBox1.Items.Clear();
                for (int i = 0; i < channel_count; i++)
                {
                    object tmp = new object();
                    object name = new object();//object是一个类，temp是类中的有个对象，
                    Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CHANNEL_NAME, out name);//该功能可用于检索有关某个频道的某些信息

                    //渠道  i 您感兴趣的频道号。频道号是从0（零）开始到canGetNumberOfChannels（）减1后返回的值的整数。
                    // Canlib.canCHANNELDATA_CHANNEL_NAME	此参数指定要获取指定通道的数据。该值是常量canCHANNELDATA_xxx之一。该区域接收一个带有明文名称通道的以零结尾的字符串
                    //缓冲	 name   要接收数据的缓冲区的地址

                    Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_TRANS_TYPE, out tmp);
                    comboBox1.Items.Add(name.ToString());//把属性转换为字符串的格式，且增加到COMBOX1中
                    comboBox1.SelectedIndex = 0;         //选择第一个通道

                }



            }
            else
            {
                MessageBox.Show("Read \"Canlib.canGetNumberOfChannels\" is error!");
                return;


            }




        }


        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            Kvaser_Can_Init();
            comboBox2.SelectedIndex = 0;//默认的选择文本的第一行
                                        // DefWndProc(ref Message m);
            ovalShape1.FillColor = Color.Red;
            ovalShape2.FillColor = Color.Red;


        }




        /**********************            CAN 接收                         **********************************************/
        int received_can_id1 = 0;

        byte[] can_receive_data1 = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0, };

        int received_dlc1 = 0;
        int received_can_flag1 = 0;
        long received_can_time1 = 0;

        //   int  received_can_id2 = 0;
        byte[] can_receive_data2 = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0, };
        int received_dlc2 = 0;
        //  int received_can_flag2 = 0;
        //  long received_can_time2 = 0;


        protected override void DefWndProc(ref Message m)
        {

            switch (m.Msg)
            {
                case Canlib.WM__CANLIB:

                    if (Canlib.canRead(handl, out received_can_id1, can_receive_data1, out received_dlc1, out received_can_flag1, out received_can_time1) == Canlib.canStatus.canOK)
                    {
                        // if(!can_stop)
                        // AddMsg(can_id, can_dlc, can_data, can_time);

                        if (received_can_id1 == 0X302) //收到302的数据
                        {
                            //if(count==0)
                            timer3.Enabled = true;
                            test_302();
                            label56.Text = count.ToString();

                        }

                        else if (received_can_id1 == 0X334)
                        {
                            test_334();

                        }

                        else if (received_can_id1 == 0X010)
                        {
                            test_010();

                        }

                        addmsg();





                    }

                    break;

                default:
                    base.DefWndProc(ref m);///调用基类函数处理非自定义消息。
                    break;
            }
        }
        public void addmsg()
        {
            richTextBox2.AppendText(string.Format("{0:X2}", received_can_id1) + " " + string.Format("{0:X2}", received_dlc1)
                                            + " " + string.Format("{0:X2}", can_receive_data1[0])
                                            + " " + string.Format("{0:X2}", can_receive_data1[1])
                                            + " " + string.Format("{0:X2}", can_receive_data1[2])
                                            + " " + string.Format("{0:X2}", can_receive_data1[3])
                                            + " " + string.Format("{0:X2}", can_receive_data1[4])
                                            + " " + string.Format("{0:X2}", can_receive_data1[5])
                                            + " " + string.Format("{0:X2}", can_receive_data1[6])
                                            + " " + string.Format("{0:X2}", can_receive_data1[7])
                                            + " " + (received_can_time1 / 1000).ToString("d3") + "." + (received_can_time1 % 1000).ToString("d3")
                                           + "\r\n");

            richTextBox2.ScrollToCaret();//这句是把数据一直放在窗口的最下面
            if (richTextBox2.Lines.Length > 50)
                richTextBox2.Clear();
        }

        /********************   初始化LED ***************************************/
        public void init_led()

        {
            ovalShape1.FillColor = Color.Red;
            ovalShape2.FillColor = Color.Red;
            ovalShape3.FillColor = Color.Red;
            ovalShape4.FillColor = Color.Red;
            ovalShape5.FillColor = Color.Red;
            ovalShape6.FillColor = Color.Red;
            ovalShape7.FillColor = Color.Red;
            ovalShape8.FillColor = Color.Red;
            ovalShape9.FillColor = Color.Red;
            ovalShape10.FillColor = Color.Red;
            ovalShape11.FillColor = Color.Red;


        }

        /********************   PIN码生成SK ***************************************/
        #region PIN To SK
        private void textBox35_Leave(object sender, EventArgs e)
        {
            BcmPin[0] = int.Parse(Regex.Replace(textBox35.Text, @"(?i)[^a-f\d\s]+", ""));
        }
        private void textBox36_Leave(object sender, EventArgs e)
        {
            BcmPin[1] = int.Parse(Regex.Replace(textBox36.Text, @"(?i)[^a-f\d\s]+", ""));
        }
        private void textBox37_Leave(object sender, EventArgs e)
        {
            BcmPin[2] = int.Parse(Regex.Replace(textBox37.Text, @"(?i)[^a-f\d\s]+", ""));
        }
        private void textBox38_Leave(object sender, EventArgs e)
        {
            BcmPin[3] = int.Parse(Regex.Replace(textBox38.Text, @"(?i)[^a-f\d\s]+", ""));
            AppLearnKeyGenerate(BcmPin, BcmKey);
            textBox39.Text = BcmKey[0].ToString();
            textBox40.Text = BcmKey[1].ToString();
            textBox41.Text = BcmKey[2].ToString();
            textBox42.Text = BcmKey[3].ToString();
            textBox43.Text = BcmKey[4].ToString();
            textBox44.Text = BcmKey[5].ToString();
        }

        private void AppLearnKeyGenerate(int[] PIN, int[] KEY)
        {
            uint ucStep;
            int[] ucSkSeedData = new int[4];
            uint KeyId = 0x09;
            ucSkSeedData[0] = PIN[0];
            ucSkSeedData[1] = PIN[1];
            ucSkSeedData[2] = PIN[2];
            ucSkSeedData[3] = PIN[3];
            for (ucStep = 0; ucStep < KeyId; ucStep++)
            {
                KEY[0] = (char)((ucSkSeedData[0] >> 3) | (((ucSkSeedData[3] & 0x07) << 5) & 0xE0));
                KEY[1] = (char)((ucSkSeedData[1] >> 3) | (((ucSkSeedData[0] & 0x07) << 5) & 0xE0));
                KEY[2] = (char)((ucSkSeedData[2] >> 3) | (((ucSkSeedData[1] & 0x07) << 5) & 0xE0));
                KEY[3] = (char)((ucSkSeedData[3] >> 3) | (((ucSkSeedData[2] & 0x07) << 5) & 0xE0));
                ucSkSeedData[0] = KEY[0];
                ucSkSeedData[1] = KEY[1];
                ucSkSeedData[2] = KEY[2];
                ucSkSeedData[3] = KEY[3];
            }
            KEY[4] = KEY[0] + KEY[1];
            KEY[5] = KEY[2] + KEY[3];
        }

        private void fill_SKExtern(int[] SK)
        {
            int[] vcu_sk_extend = new int[16];

            vcu_sk_extend[0] = SK[5];
            vcu_sk_extend[1] = SK[0];
            vcu_sk_extend[2] = (SK[3] ^ 0x53);
            vcu_sk_extend[3] = SK[1];
            vcu_sk_extend[4] = (SK[0] ^ 0x53);
            vcu_sk_extend[5] = SK[2];
            vcu_sk_extend[6] = SK[3];
            vcu_sk_extend[7] = SK[4];
            vcu_sk_extend[8] = SK[5];
            vcu_sk_extend[9] = SK[0];
            vcu_sk_extend[10] = (SK[3] ^ 0x53);
            vcu_sk_extend[11] = SK[1];
            vcu_sk_extend[12] = (SK[0] ^ 0x53);
            vcu_sk_extend[13] = SK[2];
            vcu_sk_extend[14] = SK[3];
            vcu_sk_extend[15] = SK[4];
        }

        private void fill_AESInpt(int[] rd)
        {
            int[] aes_input = new int[16];

            aes_input[0] = rd[0] ^ 0x53;
            aes_input[1] = rd[0];
            aes_input[2] = rd[1] ^ 0x53;
            aes_input[3] = rd[1];
            aes_input[4] = rd[2] ^ 0x53;
            aes_input[5] = rd[2];
            aes_input[6] = rd[3] ^ 0x53;
            aes_input[7] = rd[3];
            aes_input[8] = rd[0] ^ 0x53;
            aes_input[9] = rd[0];
            aes_input[10] = rd[1] ^ 0x53;
            aes_input[11] = rd[1];
            aes_input[12] = rd[2] ^ 0x53;
            aes_input[13] = rd[2];
            aes_input[14] = rd[3] ^ 0x53;
            aes_input[15] = rd[3];
        }
        #endregion

        //AES加密
        public static string AesEncrypt(string value, string key, string iv = "")
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (key == null) throw new Exception("未将对象引用设置到对象的实例。");
            if (key.Length < 16) throw new Exception("指定的密钥长度不能少于16位。");
            if (key.Length > 32) throw new Exception("指定的密钥长度不能多于32位。");
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) throw new Exception("指定的密钥长度不明确。");
            if (!string.IsNullOrEmpty(iv))
            {
                if (iv.Length < 16) throw new Exception("指定的向量长度不能少于16位。");
            }

            var _keyByte = Encoding.UTF8.GetBytes(key);
            var _valueByte = Encoding.UTF8.GetBytes(value);
            using (var aes = new RijndaelManaged())
            {
                aes.IV = !string.IsNullOrEmpty(iv) ? Encoding.UTF8.GetBytes(iv) : Encoding.UTF8.GetBytes(key.Substring(0, 16));
                aes.Key = _keyByte;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var cryptoTransform = aes.CreateEncryptor();
                var resultArray = cryptoTransform.TransformFinalBlock(_valueByte, 0, _valueByte.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        //AES解密
        public static string AesDecrypt(string value, string key, string iv = "")
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (key == null) throw new Exception("未将对象引用设置到对象的实例。");
            if (key.Length < 16) throw new Exception("指定的密钥长度不能少于16位。");
            if (key.Length > 32) throw new Exception("指定的密钥长度不能多于32位。");
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) throw new Exception("指定的密钥长度不明确。");
            if (!string.IsNullOrEmpty(iv))
            {
                if (iv.Length < 16) throw new Exception("指定的向量长度不能少于16位。");
            }

            var _keyByte = Encoding.UTF8.GetBytes(key);
            var _valueByte = Convert.FromBase64String(value);
            using (var aes = new RijndaelManaged())
            {
                aes.IV = !string.IsNullOrEmpty(iv) ? Encoding.UTF8.GetBytes(iv) : Encoding.UTF8.GetBytes(key.Substring(0, 16));
                aes.Key = _keyByte;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var cryptoTransform = aes.CreateDecryptor();
                var resultArray = cryptoTransform.TransformFinalBlock(_valueByte, 0, _valueByte.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
        }

        /************************** 点亮LED **************************************/
        public void test_302()
        {



            if ((can_receive_data1[0] & 0x01) == 0x01)                   //遥控解锁CAN 信号
            {
                ovalShape1.FillColor = Color.Blue;


            }
            else
            {
                ovalShape1.FillColor = Color.Red;

            }



            if ((can_receive_data1[0] & 0x02) == 0x02)                  //遥控闭锁CAN 信号
            {
                ovalShape2.FillColor = Color.Blue;


            }
            else
            {
                ovalShape2.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x04) == 0x04)                  //遥控寻车
            {
                ovalShape3.FillColor = Color.Blue;


            }
            else
            {
                ovalShape3.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x08) == 0x08)                  //遥控车窗下降
            {
                ovalShape4.FillColor = Color.Blue;


            }
            else
            {
                ovalShape4.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x10) == 0x10)                  //遥控车窗上升
            {
                ovalShape5.FillColor = Color.Blue;


            }
            else
            {
                ovalShape5.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x20) == 0x20)                  //遥控后备箱解锁
            {
                ovalShape6.FillColor = Color.Blue;


            }
            else
            {
                ovalShape6.FillColor = Color.Red;

            }



            int keystatus = can_receive_data1[0] & 0xc0;
            switch (keystatus)
            {
                case 0x00:
                    {
                        ovalShape7.FillColor = Color.Red;//OFF
                        ovalShape8.FillColor = Color.Red;
                        ovalShape9.FillColor = Color.Red;
                    }
                    break;
                case 0x40:
                    {
                        ovalShape7.FillColor = Color.Blue;//ACC
                        ovalShape8.FillColor = Color.Red;
                        ovalShape9.FillColor = Color.Red;

                    }
                    break;
                case 0x80:
                    {

                        ovalShape7.FillColor = Color.Red;//ON
                        ovalShape8.FillColor = Color.Blue;
                        ovalShape9.FillColor = Color.Red;
                    }
                    break;
                case 0xc0:
                    {
                        ovalShape7.FillColor = Color.Red;//start
                        ovalShape8.FillColor = Color.Red;
                        ovalShape9.FillColor = Color.Blue;

                    }
                    break;

            }

            int keyswarn = can_receive_data1[1] & 0x07;
            switch (keyswarn)
            {
                case 0x01:
                    {
                        ovalShape10.FillColor = Color.Blue;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Red;//电池电量低
                        ovalShape12.FillColor = Color.Red;//系统故障
                        ovalShape13.FillColor = Color.Red;//车内要说离开
                        ovalShape14.FillColor = Color.Red;//停车请挂N档
                        ovalShape15.FillColor = Color.Red;//请带好钥匙
                        ovalShape16.FillColor = Color.Red;//启动条件不满足
                    }
                    break;

                case 0x02:
                    {
                        ovalShape10.FillColor = Color.Red;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Blue;//电池电量低
                        ovalShape12.FillColor = Color.Red;//系统故障
                        ovalShape13.FillColor = Color.Red;//车内要说离开
                        ovalShape14.FillColor = Color.Red;//停车请挂N档
                        ovalShape15.FillColor = Color.Red;//请带好钥匙
                        ovalShape16.FillColor = Color.Red;//启动条件不满足
                    }
                    break;

                case 0x03:
                    {
                        ovalShape10.FillColor = Color.Red;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Red;//电池电量低
                        ovalShape12.FillColor = Color.Blue;//系统故障
                        ovalShape13.FillColor = Color.Red;//车内要说离开
                        ovalShape14.FillColor = Color.Red;//停车请挂N档
                        ovalShape15.FillColor = Color.Red;//请带好钥匙
                        ovalShape16.FillColor = Color.Red;//启动条件不满足
                    }
                    break;

                case 0x04:
                    {
                        ovalShape10.FillColor = Color.Red;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Red;//电池电量低
                        ovalShape12.FillColor = Color.Red;//系统故障
                        ovalShape13.FillColor = Color.Blue;//车内要说离开
                        ovalShape14.FillColor = Color.Red;//停车请挂N档
                        ovalShape15.FillColor = Color.Red;//请带好钥匙
                        ovalShape16.FillColor = Color.Red;//启动条件不满足
                    }
                    break;

                case 0x05:
                    {
                        ovalShape10.FillColor = Color.Red;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Red;//电池电量低
                        ovalShape12.FillColor = Color.Red;//系统故障
                        ovalShape13.FillColor = Color.Red;//车内要说离开
                        ovalShape14.FillColor = Color.Blue;//停车请挂N档
                        ovalShape15.FillColor = Color.Red;//请带好钥匙
                        ovalShape16.FillColor = Color.Red;//启动条件不满足
                    }
                    break;

                case 0x06:
                    {
                        ovalShape10.FillColor = Color.Red;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Red;//电池电量低
                        ovalShape12.FillColor = Color.Red;//系统故障
                        ovalShape13.FillColor = Color.Red;//车内要说离开
                        ovalShape14.FillColor = Color.Red;//停车请挂N档
                        ovalShape15.FillColor = Color.Blue;//请带好钥匙
                        ovalShape16.FillColor = Color.Red;//启动条件不满足
                    }
                    break;

                case 0x07:
                    {
                        ovalShape10.FillColor = Color.Red;//未检测你到钥匙
                        ovalShape11.FillColor = Color.Red;//电池电量低
                        ovalShape12.FillColor = Color.Red;//系统故障
                        ovalShape13.FillColor = Color.Red;//车内要说离开
                        ovalShape14.FillColor = Color.Red;//停车请挂N档
                        ovalShape15.FillColor = Color.Red;//请带好钥匙
                        ovalShape16.FillColor = Color.Blue;//启动条件不满足
                    }
                    break;




            }


            int AlarmModeStatus = can_receive_data1[1] & 0x18;//防盗报警

            if (AlarmModeStatus == 0x08)

            {
                ovalShape17.FillColor = Color.Blue;//Alarm
                ovalShape18.FillColor = Color.Red;
                ovalShape19.FillColor = Color.Red;
                ovalShape29.FillColor = Color.Red;
            }
            else if (AlarmModeStatus == 0x10)
            {
                ovalShape17.FillColor = Color.Red;
                ovalShape18.FillColor = Color.Blue;// Full Alarm
                ovalShape19.FillColor = Color.Red;
                ovalShape29.FillColor = Color.Red;

            }
            else if (AlarmModeStatus == 0x18)
            {
                ovalShape17.FillColor = Color.Red;
                ovalShape18.FillColor = Color.Red;
                ovalShape19.FillColor = Color.Blue;// Full Alarm
                ovalShape29.FillColor = Color.Red;


            }
            else
            {
                ovalShape17.FillColor = Color.Red;
                ovalShape18.FillColor = Color.Red;
                ovalShape19.FillColor = Color.Red;// Full Alarm
                ovalShape29.FillColor = Color.Blue;

            }

            /************ 与VCU认证成功      ******************/

            int VcuStatus = can_receive_data1[1] & 0x60;

            if (VcuStatus == 0x20)
            {
                ovalShape20.FillColor = Color.Red;
                ovalShape21.FillColor = Color.Blue;//失败的灯点亮


            }

            else
            {
                ovalShape20.FillColor = Color.Blue;//成功点亮
                ovalShape21.FillColor = Color.Red;
            }


            if ((can_receive_data1[2] & 0x01) == 0x01)                   //左前门碰开挂输入
            {
                ovalShape22.FillColor = Color.Blue;


            }
            else
            {
                ovalShape22.FillColor = Color.Red;

            }


            if ((can_receive_data1[2] & 0x02) == 0x02)                   //右前门碰开挂输入
            {
                ovalShape23.FillColor = Color.Blue;


            }
            else
            {
                ovalShape23.FillColor = Color.Red;

            }

            if ((can_receive_data1[2] & 0x04) == 0x04)                   //左后门碰开挂输入
            {
                ovalShape24.FillColor = Color.Blue;


            }
            else
            {
                ovalShape24.FillColor = Color.Red;

            }

            if ((can_receive_data1[2] & 0x08) == 0x08)                   //右后门碰开挂输入
            {
                ovalShape25.FillColor = Color.Blue;


            }
            else
            {
                ovalShape25.FillColor = Color.Red;

            }

            if ((can_receive_data1[2] & 0x10) == 0x10)                   //前舱盖开关输入
            {
                ovalShape26.FillColor = Color.Blue;


            }
            else
            {
                ovalShape26.FillColor = Color.Red;

            }

            if ((can_receive_data1[2] & 0x20) == 0x20)                   //后背箱开关输入
            {
                ovalShape27.FillColor = Color.Blue;


            }
            else
            {
                ovalShape27.FillColor = Color.Red;

            }

            /*******************       灯光输出                  ****************************/
            if ((can_receive_data1[2] & 0x80) == 0x80)                   //右转向灯
            {
                ovalShape31.FillColor = Color.Blue;


            }
            else
            {
                ovalShape31.FillColor = Color.Red;

            }


            if ((can_receive_data1[3] & 0x01) == 0x01)                   //左转向灯
            {
                ovalShape32.FillColor = Color.Blue;


            }
            else
            {
                ovalShape32.FillColor = Color.Red;

            }

            if ((can_receive_data1[3] & 0x02) == 0x02)                   //后雾灯输出
            {
                ovalShape33.FillColor = Color.Blue;


            }
            else
            {
                ovalShape33.FillColor = Color.Red;

            }

            if ((can_receive_data1[3] & 0x04) == 0x04)                   //左位置向灯
            {
                ovalShape34.FillColor = Color.Blue;


            }
            else
            {
                ovalShape34.FillColor = Color.Red;

            }

            if ((can_receive_data1[3] & 0x08) == 0x08)                   //右位置向灯
            {
                ovalShape35.FillColor = Color.Blue;


            }
            else
            {
                ovalShape35.FillColor = Color.Red;

            }

            if ((can_receive_data1[3] & 0x40) == 0x40)                   //防盗指示灯
            {
                ovalShape37.FillColor = Color.Blue;


            }
            else
            {
                ovalShape37.FillColor = Color.Red;

            }

            if ((can_receive_data1[3] & 0x40) == 0x40)                   //防盗指示灯
            {
                ovalShape37.FillColor = Color.Blue;


            }
            else
            {
                ovalShape37.FillColor = Color.Red;

            }

            if ((can_receive_data1[3] & 0x80) == 0x80)                   //近光灯
            {
                ovalShape38.FillColor = Color.Blue;


            }
            else
            {
                ovalShape38.FillColor = Color.Red;

            }

            if ((can_receive_data1[4] & 0x01) == 0x01)                   //左日行灯
            {
                ovalShape39.FillColor = Color.Blue;


            }
            else
            {
                ovalShape39.FillColor = Color.Red;

            }

            if ((can_receive_data1[4] & 0x02) == 0x02)                   //右日行灯
            {
                ovalShape40.FillColor = Color.Blue;


            }
            else
            {
                ovalShape40.FillColor = Color.Red;

            }

            if ((can_receive_data1[4] & 0x04) == 0x04)                   //左前雾灯
            {
                ovalShape41.FillColor = Color.Blue;


            }
            else
            {
                ovalShape41.FillColor = Color.Red;

            }


            if ((can_receive_data1[4] & 0x08) == 0x08)                   //右前雾灯
            {
                ovalShape42.FillColor = Color.Blue;


            }
            else
            {
                ovalShape42.FillColor = Color.Red;

            }

            if ((can_receive_data1[4] & 0x10) == 0x10)                   //远光灯输出
            {
                ovalShape43.FillColor = Color.Blue;


            }
            else
            {
                ovalShape43.FillColor = Color.Red;

            }

            if ((can_receive_data1[4] & 0x20) == 0x20)                   //位置灯未关报警
            {
                ovalShape44.FillColor = Color.Blue;


            }
            else
            {
                ovalShape44.FillColor = Color.Red;

            }

            /***************************雨刮***********************/

            /***************************雨刮***********************/
            if ((can_receive_data1[4] & 0x40) == 0x40)                   //雨刮低速
            {
                ovalShape45.FillColor = Color.Blue;


            }
            else
            {
                ovalShape45.FillColor = Color.Red;

            }

            if ((can_receive_data1[4] & 0x80) == 0x80)                   //雨刮高速
            {
                ovalShape46.FillColor = Color.Blue;


            }
            else
            {
                ovalShape46.FillColor = Color.Red;

            }

            if ((can_receive_data1[5] & 0x01) == 0x01)                   //雨刮洗涤
            {
                ovalShape47.FillColor = Color.Blue;


            }
            else
            {
                ovalShape47.FillColor = Color.Red;

            }

            /***************************T_BOX***********************/

            /***************************T_BO***********************/
            if ((can_receive_data1[5] & 0x02) == 0x02)                   //雨刮洗涤
            {
                ovalShape48.FillColor = Color.Blue;
                ovalShape49.FillColor = Color.Red;



            }
            else
            {
                ovalShape48.FillColor = Color.Red;
                ovalShape49.FillColor = Color.Blue;


            }




            /***************************锁的状态***********************/

            /***************************锁的状态***********************/

            int LockStatus = can_receive_data1[3] & 0X30;

            if (LockStatus == 0X10)
            {
                ovalShape30.FillColor = Color.Red;//解锁
                ovalShape36.FillColor = Color.Blue;//闭锁

            }

            else if (LockStatus == 0X20)

            {
                ovalShape30.FillColor = Color.Blue;//解锁
                ovalShape36.FillColor = Color.Red;//闭锁

            }
            else
            {
                ovalShape30.FillColor = Color.Red;//解锁
                ovalShape36.FillColor = Color.Red;//闭锁

            }







        }

        /*****************************    VCU认证               *******************************************************/
        /*****************************    VCU认证               *******************************************************/


        byte[] Vcurandomudata = new byte[8] { 0x02, 0x80, 0x11, 0x22, 0x33, 0x44, 0x55, 0x55 };



        public void send_Vcurandomudata()
        {
            int id = 0X020;
            long time;
            if (Canlib.canWrite(handl, id, data, 8, 0x0000) == Canlib.canStatus.canOK)
            {
                time = Canlib.canReadTimer(handl);
                richTextBox1.AppendText(String.Format("{0:X2}", id) + " " + 8
                                            + " " + String.Format("{0:X2}", Vcurandomudata[0])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[1])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[2])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[3])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[4])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[5])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[6])
                                            + " " + String.Format("{0:X2}", Vcurandomudata[7])
                                            + " " + (time / 1000).ToString("d3") + "." + (time % 1000).ToString("d3") + "\r\n");//"\r\n"是换行的意思
                //richTextBox1.ScrollToCaret();//这句是把数据一直放在窗口的最下面
                //if (richTextBox1.Lines.Length > 50)
                //    richTextBox1.Clear();
            }
        }

        public void test_010()
        {
            if (can_receive_data1[0] == 0x02)
            {
                BcmRandom();
                send_Vcurandomudata();
                VcuRandom();
            }


        }

        public void BcmRandom()
        {
            textBox2.Text = can_receive_data1[0].ToString("X2");
            textBox3.Text = can_receive_data1[1].ToString("X2");
            textBox4.Text = can_receive_data1[2].ToString("X2");
            textBox5.Text = can_receive_data1[3].ToString("X2");
            textBox6.Text = can_receive_data1[4].ToString("X2");

            BcmRan[0] = int.Parse(Regex.Replace(textBox2.Text, @"(?i)[^a-f\d\s]+", ""));
            BcmRan[1] = int.Parse(Regex.Replace(textBox2.Text, @"(?i)[^a-f\d\s]+", ""));
            BcmRan[2] = int.Parse(Regex.Replace(textBox2.Text, @"(?i)[^a-f\d\s]+", ""));
            BcmRan[3] = int.Parse(Regex.Replace(textBox2.Text, @"(?i)[^a-f\d\s]+", ""));
            BcmRan[4] = int.Parse(Regex.Replace(textBox2.Text, @"(?i)[^a-f\d\s]+", ""));

            fill_AESInpt(BcmRan);

        }
        public void VcuRandom()
        {
            textBox7.Text = Vcurandomudata[0].ToString("X2");
            textBox8.Text = Vcurandomudata[1].ToString("X2");
            textBox9.Text = Vcurandomudata[2].ToString("X2");
            textBox10.Text = Vcurandomudata[3].ToString("X2");
            textBox11.Text = Vcurandomudata[4].ToString("X2");
            textBox12.Text = Vcurandomudata[5].ToString("X2");
        }







        public void test_334()
        {
            if ((can_receive_data1[0] & 0x01) == 0x01)                   //l喇叭输入
            {
                ovalShape50.FillColor = Color.Blue;

            }
            else
            {
                ovalShape50.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x02) == 0x02)                   //后除霜输入
            {
                ovalShape51.FillColor = Color.Blue;

            }
            else
            {
                ovalShape51.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x04) == 0x04)                   //前雾灯输入
            {
                ovalShape52.FillColor = Color.Blue;

            }
            else
            {
                ovalShape52.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x08) == 0x08)                   //后雾灯输入
            {
                ovalShape53.FillColor = Color.Blue;

            }
            else
            {
                ovalShape53.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x10) == 0x10)                   //位置灯输入
            {
                ovalShape54.FillColor = Color.Blue;

            }
            else
            {
                ovalShape54.FillColor = Color.Red;

            }


            if ((can_receive_data1[0] & 0x20) == 0x20)                   //远光灯输入
            {
                ovalShape55.FillColor = Color.Blue;

            }
            else
            {
                ovalShape55.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x40) == 0x40)                   //左转向灯输入
            {
                ovalShape56.FillColor = Color.Blue;

            }
            else
            {
                ovalShape56.FillColor = Color.Red;

            }

            if ((can_receive_data1[0] & 0x80) == 0x80)                   //近光灯输入
            {
                ovalShape57.FillColor = Color.Blue;

            }
            else
            {
                ovalShape57.FillColor = Color.Red;

            }

            if ((can_receive_data1[1] & 0x01) == 0x01)                   //右转向灯输入
            {
                ovalShape58.FillColor = Color.Blue;

            }
            else
            {
                ovalShape58.FillColor = Color.Red;

            }


            if ((can_receive_data1[1] & 0x02) == 0x02)                   //危险灯输入
            {
                ovalShape59.FillColor = Color.Blue;

            }
            else
            {
                ovalShape59.FillColor = Color.Red;

            }

            if ((can_receive_data1[1] & 0x04) == 0x04)                   //左前门锁状态
            {
                ovalShape60.FillColor = Color.Blue;

            }
            else
            {
                ovalShape60.FillColor = Color.Yellow;

            }

            if ((can_receive_data1[1] & 0x08) == 0x08)                   //启动开关1
            {
                ovalShape61.FillColor = Color.Blue;

            }
            else
            {
                ovalShape61.FillColor = Color.Yellow;

            }


            if ((can_receive_data1[1] & 0x10) == 0x10)                   //启动开关2
            {
                ovalShape62.FillColor = Color.Blue;

            }
            else
            {
                ovalShape62.FillColor = Color.Yellow;

            }

            int CenterLock = can_receive_data1[1] & 0x60;
            if (CenterLock == 0x20)                   //中控锁输入
            {
                ovalShape63.FillColor = Color.Blue;
                ovalShape64.FillColor = Color.Red;

            }
            else if (CenterLock == 0x40)
            {
                ovalShape64.FillColor = Color.Blue;
                ovalShape63.FillColor = Color.Red;

            }

            else
            {
                ovalShape63.FillColor = Color.Red;
                ovalShape64.FillColor = Color.Red;

            }

            if ((can_receive_data1[1] & 0x80) == 0x80)
            {
                ovalShape65.FillColor = Color.Blue;
            }

            else
            {
                ovalShape65.FillColor = Color.Red;
            }

            if ((can_receive_data1[2] & 0x01) == 0x01)
            {
                ovalShape66.FillColor = Color.Blue;
            }

            else
            {
                ovalShape66.FillColor = Color.Red;
            }

            if ((can_receive_data1[2] & 0x02) == 0x02)//ACC继电器
            {
                ovalShape67.FillColor = Color.Blue;
            }

            else
            {
                ovalShape67.FillColor = Color.Red;
            }

            if ((can_receive_data1[2] & 0x04) == 0x04)//ON继电器
            {
                ovalShape68.FillColor = Color.Blue;
            }

            else
            {
                ovalShape68.FillColor = Color.Red;
            }

            if ((can_receive_data1[2] & 0x10) == 0x10)//左转向灯
            {
                ovalShape69.FillColor = Color.Blue;
            }

            else
            {
                ovalShape69.FillColor = Color.Red;
            }

            if ((can_receive_data1[2] & 0x20) == 0x20)//转右向灯
            {
                ovalShape70.FillColor = Color.Blue;
            }

            else
            {
                ovalShape70.FillColor = Color.Red;
            }

            if ((can_receive_data1[2] & 0x40) == 0x40)//危险指示灯输出
            {
                ovalShape71.FillColor = Color.Blue;
            }

            else
            {
                ovalShape71.FillColor = Color.Red;
            }


            if ((can_receive_data1[2] & 0x80) == 0x80)//电池节能
            {
                ovalShape84.FillColor = Color.Blue;
            }

            else
            {
                ovalShape84.FillColor = Color.Red;
            }


            if ((can_receive_data1[3] & 0x01) == 0x01)//顶灯输出
            {
                ovalShape72.FillColor = Color.Blue;
            }

            else
            {
                ovalShape72.FillColor = Color.Red;
            }

            if ((can_receive_data1[3] & 0x02) == 0x02)//喇叭输出
            {
                ovalShape74.FillColor = Color.Blue;
            }

            else
            {
                ovalShape74.FillColor = Color.Red;
            }

            if ((can_receive_data1[3] & 0x04) == 0x04)//倒车灯输出
            {
                ovalShape73.FillColor = Color.Blue;
            }

            else
            {
                ovalShape73.FillColor = Color.Red;
            }

            if ((can_receive_data1[3] & 0x08) == 0x08)//刹车灯输出
            {
                ovalShape75.FillColor = Color.Blue;
            }

            else
            {
                ovalShape75.FillColor = Color.Red;
            }

            if ((can_receive_data1[3] & 0x10) == 0x10)//后背箱解锁
            {
                ovalShape76.FillColor = Color.Blue;
            }

            else
            {
                ovalShape76.FillColor = Color.Red;
            }

            if ((can_receive_data1[4] & 0x02) == 0x02) //SSB绿色
            {
                ovalShape77.FillColor = Color.Blue;
            }

            else
            {
                ovalShape77.FillColor = Color.Red;
            }

            if ((can_receive_data1[4] & 0x04) == 0x04) //SSB琥珀
            {
                ovalShape78.FillColor = Color.Blue;
            }

            else
            {
                ovalShape78.FillColor = Color.Red;
            }

            if ((can_receive_data1[4] & 0x08) == 0x08) //天线状态
            {
                ovalShape79.FillColor = Color.Blue;
            }

            else
            {
                ovalShape79.FillColor = Color.Red;
            }

            if ((can_receive_data1[4] & 0x08) == 0x08) //刹车输入
            {
                ovalShape80.FillColor = Color.Blue;
            }

            else
            {
                ovalShape80.FillColor = Color.Red;
            }

            if ((can_receive_data1[4] & 0x08) == 0x08) //刹车输入
            {
                ovalShape80.FillColor = Color.Blue;
            }

            else
            {
                ovalShape80.FillColor = Color.Red;
            }

            if ((can_receive_data1[5] & 0x01) == 0x01) //雨喷水
            {
                ovalShape81.FillColor = Color.Blue;
            }

            else
            {
                ovalShape81.FillColor = Color.Red;
            }

            if ((can_receive_data1[5] & 0x02) == 0x02) //雨刮间歇
            {
                ovalShape82.FillColor = Color.Blue;
            }

            else
            {
                ovalShape82.FillColor = Color.Red;
            }

            if ((can_receive_data1[5] & 0x04) == 0x04) //雨刮高速输入
            {
                ovalShape83.FillColor = Color.Blue;
            }

            else
            {
                ovalShape83.FillColor = Color.Red;
            }

            if ((can_receive_data1[5] & 0x08) == 0x08) //雨刮低速输入
            {
                ovalShape85.FillColor = Color.Blue;
            }

            else
            {
                ovalShape85.FillColor = Color.Red;
            }
        }




        /**********************            CAN 发送                           **********************************************/
        byte[] data = new byte[8] { 0x00, 0x00, 0x000, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] data1 = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] data_ABS = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] data_avn = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] data_acu = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public void send_msg_vcu()
        {
            int id = 0X3C0;
            long time;
            if (Canlib.canWrite(handl, id, data, 8, 0x0000) == Canlib.canStatus.canOK)
            {
                time = Canlib.canReadTimer(handl);
                richTextBox1.AppendText(String.Format("{0:X2}", id) + " " + 8
                                            + " " + String.Format("{0:X2}", data[0])
                                            + " " + String.Format("{0:X2}", data[1])
                                            + " " + String.Format("{0:X2}", data[2])
                                            + " " + String.Format("{0:X2}", data[3])
                                            + " " + String.Format("{0:X2}", data[4])
                                            + " " + String.Format("{0:X2}", data[5])
                                            + " " + String.Format("{0:X2}", data[6])
                                            + " " + String.Format("{0:X2}", data[7])
                                            + " " + (time / 1000).ToString("d3") + "." + (time % 1000).ToString("d3") + "\r\n");//"\r\n"是换行的意思
                //richTextBox1.ScrollToCaret();//这句是把数据一直放在窗口的最下面
                //if (richTextBox1.Lines.Length > 50)
                //    richTextBox1.Clear();
            }
        }
        public void send_msg_ABS()
        {
            int id = 0X226;
            long time;
            if (Canlib.canWrite(handl, id, data_ABS, 8, 0x0000) == Canlib.canStatus.canOK)
            {
                time = Canlib.canReadTimer(handl);
                richTextBox1.AppendText(String.Format("{0:X2}", id) + " " + 8
                                            + " " + String.Format("{0:X2}", data_ABS[0])
                                            + " " + String.Format("{0:X2}", data_ABS[1])
                                            + " " + String.Format("{0:X2}", data_ABS[2])
                                            + " " + String.Format("{0:X2}", data_ABS[3])
                                            + " " + String.Format("{0:X2}", data_ABS[4])
                                            + " " + String.Format("{0:X2}", data_ABS[5])
                                            + " " + String.Format("{0:X2}", data_ABS[6])
                                            + " " + String.Format("{0:X2}", data_ABS[7])
                                            + " " + (time / 1000).ToString("d3") + "." + (time % 1000).ToString("d3") + "\r\n");//"\r\n"是换行的意思
                //richTextBox1.ScrollToCaret();//这句是把数据一直放在窗口的最下面
                //if (richTextBox1.Lines.Length > 50)
                //    richTextBox1.Clear();
            }
        }


        public void send_msg_rke()
        {
            int id = 0X302;
            long time;
            if (Canlib.canWrite(handl, id, data1, 8, 0x0000) == Canlib.canStatus.canOK)
            {
                time = Canlib.canReadTimer(handl);
                richTextBox1.AppendText(String.Format("{0:X2}", id) + " " + 8
                                            + " " + String.Format("{0:X2}", data1[0])
                                            + " " + String.Format("{0:X2}", data1[1])
                                            + " " + String.Format("{0:X2}", data1[2])
                                            + " " + String.Format("{0:X2}", data1[3])
                                            + " " + String.Format("{0:X2}", data1[4])
                                            + " " + String.Format("{0:X2}", data1[5])
                                            + " " + String.Format("{0:X2}", data1[6])
                                            + " " + String.Format("{0:X2}", data1[7])
                                            + " " + (time / 1000).ToString("d3") + "." + (time % 1000).ToString("d3") + "\r\n");//"\r\n"是换行的意思
                //richTextBox1.ScrollToCaret();//这句是把数据一直放在窗口的最下面

                //if (richTextBox1.Lines.Length > 50)
                //    richTextBox1.Clear();
            }
        }


        public void send_meg_avn()
        {
            int id = 0X52b;
            long time;
            if (Canlib.canWrite(handl, id, data_avn, 8, 0x0000) == Canlib.canStatus.canOK)
            {
                time = Canlib.canReadTimer(handl);
                richTextBox1.AppendText(String.Format("{0:X2}", id) + " " + 8
                                            + " " + String.Format("{0:X2}", data_avn[0])
                                            + " " + String.Format("{0:X2}", data_avn[1])
                                            + " " + String.Format("{0:X2}", data_avn[2])
                                            + " " + String.Format("{0:X2}", data_avn[3])
                                            + " " + String.Format("{0:X2}", data_avn[4])
                                            + " " + String.Format("{0:X2}", data_avn[5])
                                            + " " + String.Format("{0:X2}", data_avn[6])
                                            + " " + String.Format("{0:X2}", data_avn[7])
                                            + " " + (time / 1000).ToString("d3") + "." + (time % 1000).ToString("d3") + "\r\n");//"\r\n"是换行的意思
                richTextBox1.ScrollToCaret();//这句是把数据一直放在窗口的最下面

                if (richTextBox1.Lines.Length > 50)
                    richTextBox1.Clear();
            }

        }


        public void send_meg_acu()
        {
            int id = 0X385;
            long time;
            if (Canlib.canWrite(handl, id, data_acu, 8, 0x0000) == Canlib.canStatus.canOK)
            {
                time = Canlib.canReadTimer(handl);
                richTextBox1.AppendText(String.Format("{0:X2}", id) + " " + 8
                                            + " " + String.Format("{0:X2}", data_acu[0])
                                            + " " + String.Format("{0:X2}", data_acu[1])
                                            + " " + String.Format("{0:X2}", data_acu[2])
                                            + " " + String.Format("{0:X2}", data_acu[3])
                                            + " " + String.Format("{0:X2}", data_acu[4])
                                            + " " + String.Format("{0:X2}", data_acu[5])
                                            + " " + String.Format("{0:X2}", data_acu[6])
                                            + " " + String.Format("{0:X2}", data_acu[7])
                                            + " " + (time / 1000).ToString("d3") + "." + (time % 1000).ToString("d3") + "\r\n");//"\r\n"是换行的意思
                richTextBox1.ScrollToCaret();//这句是把数据一直放在窗口的最下面

                if (richTextBox1.Lines.Length > 50)
                    richTextBox1.Clear();
            }

        }



        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            send_msg_vcu();
            send_msg_rke();
            send_msg_ABS();
            send_meg_avn();
            send_meg_acu();

        }





        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void ovalShape2_Click(object sender, EventArgs e)
        {

        }

        private void ovalShape2_Click_1(object sender, EventArgs e)
        {



        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        //private void timer2_Tick(object sender, EventArgs e)
        //{
        //    Canlib.canGetBusStatistics(0,out Canlib.canBusStatistics.busLoad);
        //    if (busload > 9000)
        //    {
        //        ovalShape1.FillColor = Color.Red;

        //    }
        //}

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            /*
            int  speed = 0;
            int  vcu_speed =0;

            textBox2.Text = "0";

            vcu_speed = Convert.ToInt32(textBox1.Text);//例如输入111F

            speed = (220 / 65535) * vcu_speed;//速度最大为220，当前的速度

            textBox2.Text = Convert.ToString(speed);


            if (vcu_speed < trackBar1.Maximum)
            {
                trackBar1.Value = vcu_speed;

            }

            else
            {
                trackBar1.Value = trackBar1.Maximum;
                textBox1.Text = trackBar1.Maximum.ToString();
            }
            speed = (220 / 65535) * vcu_speed;//速度最大为220，
            */
            int value = 0;
            try
            {
                if (Convert.ToInt32(textBox1.Text) < trackBar1.Maximum)
                    trackBar1.Value = Convert.ToInt32(textBox1.Text);
                else
                {
                    trackBar1.Value = trackBar1.Maximum;
                    textBox1.Text = trackBar1.Maximum.ToString();
                }
                value = Convert.ToInt32(trackBar1.Value);
                data_ABS[2] = Convert.ToByte(value / 256);
                data_ABS[3] = Convert.ToByte(value % 256);
            }
            catch (FormatException)
            {
                MessageBox.Show("输入非法，请核查");
                textBox1.Text = "0";
            }

        }



        private void textBox2_TextChanged(object sender, EventArgs e)
        {


        }

        private void groupBox8_Enter_1(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            send_msg_ABS();
        }





        private void trackBar1_Scroll(object sender, EventArgs e)//speed
        {
            textBox1.Text = trackBar1.Value.ToString();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                timer2.Enabled = true;
            else
                timer2.Enabled = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                data_ABS[0] = Convert.ToByte(data_ABS[0] | (1 << 5));
            }
            else
            {
                data_ABS[0] = Convert.ToByte(data_ABS[0] & (~(1 << 5)));
            }
        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void label33_Click(object sender, EventArgs e)
        {

        }

        private void ovalShape35_Click(object sender, EventArgs e)
        {

        }

        private void label49_Click(object sender, EventArgs e)
        {

        }

        int count = 0;
        private void timer3_Tick(object sender, EventArgs e)
        {
            count++;
        }

        private void label68_Click(object sender, EventArgs e)
        {

        }

        private void label34_Click(object sender, EventArgs e)
        {

        }

        private void label72_Click(object sender, EventArgs e)
        {

        }

        private void label75_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Kvaser_Can_Init();//从新进行初始化
            comboBox2.SelectedIndex = 0;//默认的选择文本的第一行
                                        //    init_led();

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Canlib.canStatus stat = new Canlib.canStatus();

            if (Kavser_Can_Init_Flag == false)
            {

                handl = Canlib.canOpenChannel(comboBox1.SelectedIndex, Canlib.canOPEN_ACCEPT_VIRTUAL);//打开选择的CAN通道 

                if (comboBox2.SelectedIndex == 0)
                {
                    stat = Canlib.canSetBusParams(handl, Canlib.canBITRATE_250K, 11, 4, 4, 3, 0);
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    stat = Canlib.canSetBusParams(handl, Canlib.canBITRATE_500K, 11, 4, 4, 3, 0);
                }
                stat = Canlib.canSetNotify(handl, Handle, Canlib.canNOTIFY_RX);

                stat = Canlib.canSetBusOutputControl(handl, Kavser_Can_bus_drivertype);

                stat = Canlib.canBusOn(handl);
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;

                Kavser_Can_Init_Flag = true;
                button2.Text = "Go Off Bus";
            }
            else
            {

                Kavser_Can_Init_Flag = false;
                button2.Text = "Go On Bus";
                stat = Canlib.canBusOff(handl);
                stat = Canlib.canClose(handl);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                init_led();//初始化LED 

            }
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            send_msg_vcu();
            send_msg_rke();
            send_msg_ABS();
            send_meg_avn();
            send_meg_acu();
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                timer1.Enabled = true;
            }

            else

            {
                timer1.Enabled = false;

            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (button4.Text == "Ready On")
            {
                button4.Text = "Ready Off";
                data[0] = Convert.ToByte((data[0] & 0xfe) | 0x00);

            }
            else
            {
                button4.Text = "Ready On";
                data[0] = Convert.ToByte((data[0] & 0xfe) | 0x01);


            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (button5.Text == "R档")
            {
                button5.Text = "N档";
                data[0] = Convert.ToByte((data[0] & 0xf9) | 0x02);

            }
            else if (button5.Text == "N档")
            {
                button5.Text = "D档";
                data[0] = Convert.ToByte((data[0] & 0xf9) | 0x04);


            }

            else if (button5.Text == "D档")

            {
                button5.Text = "R档";
                data[0] = Convert.ToByte((data[0] & 0xf9) | 0x00);

            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (button6.Text == "档位有效")
            {
                button6.Text = "档位无效";
                data[0] = Convert.ToByte((data[0] & 0xef) | 0x00);

            }
            else
            {
                button6.Text = "档位有效";
                data[0] = Convert.ToByte((data[0] & 0xef) | 0x10);


            }

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            if (button8.Text == "刹车踏板有效")
            {
                button8.Text = "刹车踏板无效";
                data[5] = Convert.ToByte((data[5] & 0xfe) | 0x00);

            }
            else
            {
                button8.Text = "刹车踏板有效";
                data[5] = Convert.ToByte((data[5] & 0xfe) | 0x01);


            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            if (button9.Text == "遥控车窗上升有效")
            {
                button9.Text = "遥控车窗上升无效";
                data1[0] = Convert.ToByte((data1[0] & 0xef) | 0x00);

            }
            else
            {
                button9.Text = "遥控车窗上升有效";
                data1[0] = Convert.ToByte((data1[0] & 0xef) | 0x10);


            }
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            if (button10.Text == "遥控车窗下降有效")
            {
                button10.Text = "遥控车窗下降无效";
                data1[0] = Convert.ToByte((data1[0] & 0xf7) | 0x00);

            }
            else
            {
                button10.Text = "遥控车窗下降有效";
                data1[0] = Convert.ToByte((data1[0] & 0xf7) | 0x08);


            }
        }

        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            if (button12.Text == "车速闭锁配置无效")
            {

                button12.Text = "车速闭锁配置有效";
                data_avn[1] = Convert.ToByte((data_avn[1] & 0xfd) | 0x00);

            }
            else
            {
                button12.Text = "车速闭锁配置无效";
                data_avn[1] = Convert.ToByte((data_avn[1] & 0xfd) | 0x02);


            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (button13.Text == "碰撞信号无效")
            {

                button13.Text = "碰撞信号有效";
                data_acu[1] = Convert.ToByte((data_acu[0] & 0xbf) | 0x10);

            }
            else
            {
                button13.Text = "碰撞信号无效";
                data_acu[1] = Convert.ToByte((data_acu[0] & 0xbf) | 0x00);


            }

        }


    }
}
