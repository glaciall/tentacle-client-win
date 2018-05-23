using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cn.org.hentai.tentacle.protocol
{
    public class Command
    {
        public const byte COMMON_RESPONSE = 0x00;                // 常规应答
        public const byte HEARTBEAT = 0x01;                      // 心跳包
        public const byte CONTROL_REQUEST = 0x02;                // 请求控制
        public const byte CONTROL_RESPONSE = 0x03;               // 请求控制包的应答
        public const byte CLOSE_REQUEST = 0x04;                  // 关闭控制
        public const byte CLOSE_RESPONSE = 0x05;                 // 关闭控制包的应答（未启用）
        public const byte HID_COMMAND = 0x06;                    // 人机接口指令
        public const byte SCREENSHOT = 0x07;                     // 屏幕截图
        public const byte SET_CLIPBOARD = 0x08;                  // 设置剪切板内容
        public const byte SET_CLIPBOARD_RESPONSE = 0x09;         // 设置剪切板内容的应答
        public const byte GET_CLIPBOARD = 0x10;                  // 获取剪切板内容
        public const byte GET_CLIPBOARD_RESPONSE = 0x11;         // 获取剪切板内容的应答
        public const byte LIST_FILES = 0x12;                     // 获取文件列表
        public const byte LIST_FILES_RESPONSE = 0x13;            // 获取文件列表的应答
        public const byte DOWNLOAD_FILE = 0x14;                  // 下载文件
        public const byte DOWNLOAD_FILE_RESPONSE = 0x15;         // 下载文件的应答
        public const byte UPLOAD_FILE = 0x16;                    // 上传文件
        public const byte UPLOAD_FILE_RESPONSE = 0x17;           // 上传文件的应答

        public const byte TYPE_MOUSE = 0x01;                     // 类型：鼠标
        public const byte TYPE_KEYBOARD = 0x02;                  // 类型：键盘
    }
}
